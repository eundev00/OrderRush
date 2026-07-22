using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

// =====================================================================
//  사운드 서비스 (BGM + SFX) — 순수 재생 엔진, 앱 전역 Singleton 용도
// ---------------------------------------------------------------------
//  책임: 재생/정지/볼륨/믹서 라우팅만. 게임 이벤트는 전혀 모른다.
//   - "어떤 이벤트가 어떤 소리를 내는가"는 별도 GameAudioBinder(게임 스코프)가
//     이 서비스를 주입받아 PlaySfx()로 호출한다. (다음 단계)
//   - UI 클릭/조리/픽업 등 자연 이벤트 없는 것은 호출부에서 직접 PlaySfx().
//   - 리소스 참조는 DataKeys/PrefabKeys 방식(문자열 키 + Addressables 경로),
//     클립은 기존 IResourcesLoaderService 로 로드(캐싱됨).
//   - 볼륨은 AudioMixer 2그룹(BGM/SFX) + 노출 파라미터로 제어.
//
//  ⚠ 볼륨 저장 키는 아직 이 파일에 임시 상수로 둠 → 정식 단계에서 LocalStorageKeys 로 이동.
//     또한 ProjectLifetimeScope 등록 + AppBootstrap 에서 Initialize() 호출은 이후 단계.
// =====================================================================
public interface ISoundService
{
    UniTask Initialize();

    void PlayBgm(string audioKey, bool loop = true);
    void StopBgm(float fadeDuration = 0.5f);

    void PlaySfx(string audioKey);
    void PlaySfxAt(string audioKey, Vector3 worldPosition);

    void SetBgmVolume(float volume01);
    void SetSfxVolume(float volume01);
    void SetMuted(bool muted);

    float BgmVolume { get; }
    float SfxVolume { get; }
    bool IsMuted { get; }
}

public class SoundService : ISoundService, IDisposable
{
    private const int SfxSourceCount = 8;      // 동시 재생 SFX 소스 풀 크기
    private const float MinVolumeDb = -80f;    // 믹서 무음 dB

    // TODO: LocalStorageKeys 로 이동
    private const string BgmVolumeKey = "Sound.BgmVolume";
    private const string SfxVolumeKey = "Sound.SfxVolume";
    private const string MutedKey = "Sound.Muted";

    private readonly IResourcesLoaderService _loader;
    private readonly ILocalStorageService _storage;

    private readonly Dictionary<string, AudioClip> _clips = new();

    private GameObject _host;
    private AudioSource _bgmSource;
    private AudioSource[] _sfxSources;
    private int _sfxCursor;

    private AudioMixer _mixer;
    private AudioMixerGroup _bgmGroup;
    private AudioMixerGroup _sfxGroup;

    private CancellationTokenSource _bgmFadeCts;
    private string _currentBgmKey;

    private float _bgmVolume = 1f;
    private float _sfxVolume = 1f;
    private bool _muted;
    private bool _initialized;

    public float BgmVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;
    public bool IsMuted => _muted;

    public SoundService(IResourcesLoaderService loader, ILocalStorageService storage)
    {
        _loader = loader;
        _storage = storage;
    }

    public async UniTask Initialize()
    {
        if (_initialized)
            return;

        CreateAudioHost();

        await LoadMixer();
        await PreloadClips();

        RouteSourcesToMixer();
        LoadVolumeSettings();

        _initialized = true;
    }

    // ---------------------------------------------------------------
    //  초기화 세부
    // ---------------------------------------------------------------
    private void CreateAudioHost()
    {
        _host = new GameObject("[SoundService]");
        UnityEngine.Object.DontDestroyOnLoad(_host);

        _bgmSource = _host.AddComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;

        _sfxSources = new AudioSource[SfxSourceCount];
        for (int i = 0; i < SfxSourceCount; i++)
        {
            var src = _host.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            _sfxSources[i] = src;
        }
    }

    private async UniTask LoadMixer()
    {
        _mixer = await _loader.LoadAsync<AudioMixer>(AudioKeys.GetAudioPath(AudioKeys.GameAudioMixer));
        if (_mixer == null)
        {
            Debug.LogWarning("[SoundService] AudioMixer load failed. Volume control disabled.");
            return;
        }

        var bgmGroups = _mixer.FindMatchingGroups("BGM");
        var sfxGroups = _mixer.FindMatchingGroups("SFX");
        _bgmGroup = bgmGroups.Length > 0 ? bgmGroups[0] : null;
        _sfxGroup = sfxGroups.Length > 0 ? sfxGroups[0] : null;
    }

    private async UniTask PreloadClips()
    {
        foreach (var kvp in AudioKeys.AudioPaths)
        {
            // 믹서는 클립이 아니므로 선로딩 대상에서 제외 (LoadMixer 에서 별도 로드)
            if (kvp.Key == AudioKeys.GameAudioMixer)
                continue;

            var clip = await _loader.LoadAsync<AudioClip>(kvp.Value);
            if (clip != null)
                _clips[kvp.Key] = clip;
            else
                Debug.LogWarning($"[SoundService] Clip load failed: {kvp.Key}");
        }
    }

    private void RouteSourcesToMixer()
    {
        if (_bgmGroup != null)
            _bgmSource.outputAudioMixerGroup = _bgmGroup;

        if (_sfxGroup != null)
        {
            foreach (var src in _sfxSources)
                src.outputAudioMixerGroup = _sfxGroup;
        }
    }

    // ---------------------------------------------------------------
    //  BGM
    // ---------------------------------------------------------------
    public void PlayBgm(string audioKey, bool loop = true)
    {
        if (!_initialized)
        {
            Debug.LogWarning("[SoundService] PlayBgm before Initialize.");
            return;
        }

        if (_currentBgmKey == audioKey && _bgmSource.isPlaying)
            return;

        if (!_clips.TryGetValue(audioKey, out var clip))
        {
            Debug.LogWarning($"[SoundService] BGM clip not loaded: {audioKey}");
            return;
        }

        // 진행 중인 페이드아웃 취소 + 볼륨 원복
        _bgmFadeCts?.Cancel();
        _bgmFadeCts?.Dispose();
        _bgmFadeCts = null;
        _bgmSource.volume = 1f;

        _currentBgmKey = audioKey;
        _bgmSource.clip = clip;
        _bgmSource.loop = loop;
        _bgmSource.Play();
    }

    public void StopBgm(float fadeDuration = 0.5f)
    {
        if (!_initialized || !_bgmSource.isPlaying)
            return;

        _currentBgmKey = null;

        if (fadeDuration <= 0f)
        {
            _bgmSource.Stop();
            return;
        }

        _bgmFadeCts?.Cancel();
        _bgmFadeCts?.Dispose();
        _bgmFadeCts = new CancellationTokenSource();
        FadeOutBgm(fadeDuration, _bgmFadeCts.Token).Forget();
    }

    private async UniTaskVoid FadeOutBgm(float duration, CancellationToken ct)
    {
        float startVolume = _bgmSource.volume;
        float elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            _bgmSource.Stop();
            _bgmSource.volume = startVolume; // 소스 볼륨 원복 (실제 볼륨은 믹서가 담당)
        }
        catch (OperationCanceledException)
        {
            // 새 BGM 요청 등으로 페이드 취소됨 — 무시
        }
    }

    // ---------------------------------------------------------------
    //  SFX
    // ---------------------------------------------------------------
    public void PlaySfx(string audioKey)
    {
        if (!TryGetSfxClip(audioKey, out var clip))
            return;

        var src = NextSfxSource();
        src.transform.localPosition = Vector3.zero;
        src.spatialBlend = 0f; // 2D
        src.PlayOneShot(clip);
    }

    public void PlaySfxAt(string audioKey, Vector3 worldPosition)
    {
        if (!TryGetSfxClip(audioKey, out var clip))
            return;

        var src = NextSfxSource();
        src.transform.position = worldPosition;
        src.spatialBlend = 1f; // 3D
        src.PlayOneShot(clip);
    }

    private bool TryGetSfxClip(string audioKey, out AudioClip clip)
    {
        clip = null;
        if (!_initialized)
            return false;

        if (!_clips.TryGetValue(audioKey, out clip))
        {
            Debug.LogWarning($"[SoundService] SFX clip not loaded: {audioKey}");
            return false;
        }
        return true;
    }

    private AudioSource NextSfxSource()
    {
        // 라운드로빈 (재생 중이면 잘려도 다음 소스로 자연스레 분산됨)
        var src = _sfxSources[_sfxCursor];
        _sfxCursor = (_sfxCursor + 1) % _sfxSources.Length;
        return src;
    }

    // ---------------------------------------------------------------
    //  볼륨 (AudioMixer 노출 파라미터)
    // ---------------------------------------------------------------
    public void SetBgmVolume(float volume01)
    {
        _bgmVolume = Mathf.Clamp01(volume01);
        ApplyMixerVolume("BgmVol", _bgmVolume);
        _storage.SaveFloat(BgmVolumeKey, _bgmVolume);
    }

    public void SetSfxVolume(float volume01)
    {
        _sfxVolume = Mathf.Clamp01(volume01);
        ApplyMixerVolume("SfxVol", _sfxVolume);
        _storage.SaveFloat(SfxVolumeKey, _sfxVolume);
    }

    public void SetMuted(bool muted)
    {
        _muted = muted;
        _storage.SaveBool(MutedKey, muted);
        ApplyMixerVolume("BgmVol", _bgmVolume);
        ApplyMixerVolume("SfxVol", _sfxVolume);
    }

    private void ApplyMixerVolume(string exposedParam, float volume01)
    {
        if (_mixer == null)
            return;

        float effective = _muted ? 0f : volume01;
        _mixer.SetFloat(exposedParam, LinearToDecibel(effective));
    }

    private static float LinearToDecibel(float volume01)
    {
        // 0 → -80dB(무음), 1 → 0dB
        return volume01 <= 0.0001f ? MinVolumeDb : Mathf.Log10(volume01) * 20f;
    }

    private void LoadVolumeSettings()
    {
        _bgmVolume = _storage.LoadFloat(BgmVolumeKey, 1f);
        _sfxVolume = _storage.LoadFloat(SfxVolumeKey, 1f);
        _muted = _storage.LoadBool(MutedKey, false);

        ApplyMixerVolume("BgmVol", _bgmVolume);
        ApplyMixerVolume("SfxVol", _sfxVolume);
    }

    public void Dispose()
    {
        _bgmFadeCts?.Cancel();
        _bgmFadeCts?.Dispose();
        _bgmFadeCts = null;

        if (_host != null)
        {
            UnityEngine.Object.Destroy(_host);
            _host = null;
        }
    }
}
