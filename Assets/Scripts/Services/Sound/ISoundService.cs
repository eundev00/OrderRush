using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ISoundService
{
    UniTask Initialize();

    void PlayBgm(string audioKey, bool loop = true);
    void StopBgm(float fadeDuration = 0.5f);

    AudioSource PlaySfx(string audioKey, bool isLoop = false, float volume = 1f);
    void StopSfx(string audioKey);
    void StopSfx(AudioSource source);
    void StopAllSfx();

    void SetBgmVolume(float volume01);
    void SetSfxVolume(float volume01);
    void SetMuted(bool muted);

    float BgmVolume { get; }
    float SfxVolume { get; }
    bool IsMuted { get; }
}
