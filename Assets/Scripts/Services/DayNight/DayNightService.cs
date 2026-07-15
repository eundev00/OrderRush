using System;
using Cysharp.Threading.Tasks;
using OrderRush.Models;
using UniRx;
using UnityEngine;

namespace OrderRush.Services
{
    public class DayNightService : IDayNightService
    {
        private readonly IGameDataService _gameDataService;
        private readonly IDayProgressService _dayProgressService;

        private DayNightSettings _settings;
        private Light _outdoorLight;
        private Light _indoorLight;
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

            var outdoorLightObj = GameObject.Find("Directional Light");
            if (outdoorLightObj != null)
            {
                _outdoorLight = outdoorLightObj.GetComponent<Light>();
            }
            else
            {
                Debug.LogError("[DayNightService] Outdoor Light not found");
            }

            var indoorLightObj = GameObject.Find("Indoor Light");
            if (indoorLightObj != null)
            {
                _indoorLight = indoorLightObj.GetComponent<Light>();
            }
            else
            {
                Debug.LogWarning("[DayNightService] Indoor Light not found");
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

            float t;
            if (progress < 2f / 3f)
            {
                t = 0f;
            }
            else
            {
                t = (progress - 2f / 3f) / (1f / 3f);
                t = Mathf.Clamp01(t);
            }

            Apply(t);
        }

        private void Apply(float t)
        {
            if (_settings == null)
                return;

            if (_outdoorLight != null)
            {
                _outdoorLight.color = Color.Lerp(_settings.outdoorDay.color, _settings.outdoorNight.color, t);
                _outdoorLight.intensity = Mathf.Lerp(_settings.outdoorDay.intensity, _settings.outdoorNight.intensity, t);
            }

            Color ambientColor = Color.Lerp(_settings.outdoorDay.color, _settings.outdoorNight.color, t);
            float ambientIntensity = Mathf.Lerp(_settings.outdoorDay.intensity, _settings.outdoorNight.intensity, t);
            RenderSettings.ambientLight = ambientColor * ambientIntensity;

            if (_indoorLight != null)
            {
                _indoorLight.color = Color.Lerp(_settings.indoorDay.color, _settings.indoorNight.color, t);
                _indoorLight.intensity = Mathf.Lerp(_settings.indoorDay.intensity, _settings.indoorNight.intensity, t);
            }
        }

        public void Dispose()
        {
            _timeBarSubscription?.Dispose();
        }
    }
}
