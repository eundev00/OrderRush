using UnityEngine;

namespace OrderRush.Services
{
    public class KitchenStatService : IKitchenStatService
    {
        private readonly IGameDataService _gameDataService;
        private float _totalReducePercent;
        private float _totalOvercookExtendPercent;

        public KitchenStatService(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public void AddDurationReduce(float reducePercent)
        {
            _totalReducePercent += reducePercent;
        }

        public float GetModifiedDuration()
        {
            float baseDuration = _gameDataService.Config.ToolProcessSeconds;
            float minDuration = baseDuration * 0.5f;
            float modified = baseDuration * (1f - _totalReducePercent);
            return Mathf.Max(modified, minDuration);
        }

        public void AddOvercookExtend(float extendPercent)
        {
            _totalOvercookExtendPercent += extendPercent;
        }

        public float GetOvercookDuration()
        {
            float baseDuration = _gameDataService.Config.ToolProcessSeconds;
            float maxDuration = baseDuration * 1.5f;
            return Mathf.Min(baseDuration * (1f + _totalOvercookExtendPercent), maxDuration);
        }
    }
}
