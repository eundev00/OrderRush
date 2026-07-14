using System;
using Cysharp.Threading.Tasks;
using OrderRush.Models;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OrderRush.Services
{
    public class DayNightService : IDayNightService
    {
        private readonly IGameDataService _gameDataService;
        private readonly IDayProgressService _dayProgressService;

        private DayNightSettings _settings;
        private Light _directionalLight;
        private Volume _globalVolume;
        private ColorAdjustments _colorAdjustments;
        private IDisposable _timeBarSubscription;

        public DayNightService(
            IGameDataService gameDataService,
            IDayProgressService dayProgressService)
        {
            _gameDataService = gameDataService;
            _dayProgressService = dayProgressService;
        }

        public async UniTask Initialize()
        {
            _settings = _gameDataService.DayNightSettings;

            if (_settings == null)
            {
                Debug.LogError("[DayNightService] DayNightSettings not found");
                return;
            }

            _directionalLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (_directionalLight == null)
            {
                Debug.LogError("[DayNightService] Directional Light not found");
                return;
            }

            _globalVolume = GameObject.Find("Global Volume")?.GetComponent<Volume>();
            if (_globalVolume == null)
            {
                Debug.LogError("[DayNightService] Global Volume not found");
                return;
            }

            if (!_globalVolume.profile.TryGet(out _colorAdjustments))
            {
                Debug.LogError("[DayNightService] ColorAdjustments not found in Global Volume");
                return;
            }

            _timeBarSubscription = _dayProgressService.CurrentDayContext.TimeBarElapsed
                .Subscribe(OnTimeBarChanged);

            await UniTask.CompletedTask;
        }

        private void OnTimeBarChanged(float elapsed)
        {
            DayContext context = _dayProgressService.CurrentDayContext;
            if (context == null)
                return;

            float duration = context.TimeBarDuration;
            if (duration <= 0f)
                return;

            float progress = elapsed / duration;

            DayNightProfile targetProfile;

            if (progress < 2f / 3f)
            {
                targetProfile = _settings.day;
            }
            else
            {
                float transitionProgress = (progress - 2f / 3f) / (1f / 3f);
                transitionProgress = Mathf.Clamp01(transitionProgress);
                targetProfile = LerpProfiles(_settings.day, _settings.night, transitionProgress);
            }

            ApplyProfile(targetProfile);
        }

        private DayNightProfile LerpProfiles(DayNightProfile from, DayNightProfile to, float t)
        {
            return new DayNightProfile
            {
                lightColor = Color.Lerp(from.lightColor, to.lightColor, t),
                intensity = Mathf.Lerp(from.intensity, to.intensity, t),
                postExposure = Mathf.Lerp(from.postExposure, to.postExposure, t),
                volumeFilter = Color.Lerp(from.volumeFilter, to.volumeFilter, t),
                indoorIntensity = Mathf.Lerp(from.indoorIntensity, to.indoorIntensity, t)
            };
        }

        private void ApplyProfile(DayNightProfile profile)
        {
            if (_directionalLight != null)
            {
                _directionalLight.color = profile.lightColor;
                _directionalLight.intensity = profile.intensity;
            }

            if (_colorAdjustments != null)
            {
                _colorAdjustments.postExposure.value = profile.postExposure;
                _colorAdjustments.colorFilter.value = profile.volumeFilter;
            }
        }

        public void Dispose()
        {
            _timeBarSubscription?.Dispose();
        }
    }
}
