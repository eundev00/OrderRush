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
        private readonly Light _outdoorLight;
        private readonly Light _indoorLight;

        private DayNightSettings _settings;
        private IDisposable _timeBarSubscription;

        public DayNightService(
            IGameDataService gameDataService,
            IDayProgressService dayProgressService,
            Light outdoorLight,
            Light indoorLight)
        {
            _gameDataService = gameDataService;
            _dayProgressService = dayProgressService;
            _outdoorLight = outdoorLight;
            _indoorLight = indoorLight;
        }

        public async UniTask Initialize()
        {
            _settings = _gameDataService.DayNightSettings;

            if (_settings == null)
            {
                Debug.LogError("[DayNightService] DayNightSettings not found");
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

            float t;
            if (progress < 1f / 3f)
            {
                t = 0f;
            }
            else if (progress < 2f / 3f)
            {
                t = (progress - 1f / 3f) / (1f / 3f);
                t = Mathf.Clamp01(t);
            }
            else
            {
                t = 1f;
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
