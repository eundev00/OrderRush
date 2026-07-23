using Cysharp.Threading.Tasks;
using UnityEngine;

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
