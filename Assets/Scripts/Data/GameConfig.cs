using System.Collections.Generic;
using UnityEngine;

namespace OrderRush.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Order Rush/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Patience Settings")]
        [SerializeField] private int _enterWaitDuration = 30;
        [SerializeField] private int _orderWaitDuration = 20;
        [SerializeField] private int _foodWaitDuration = 60;
        [SerializeField] private float _patienceRecoveryAmount = 0.1f;

        [Header("Shop Settings")]
        [SerializeField] private List<int> _refreshCosts = new() { 50, 100, 150 };

        [Header("Customer Settings")]
        [SerializeField] private float _tipRatio = 0.1f;

        public int EnterWaitDuration => _enterWaitDuration;
        public int OrderWaitDuration => _orderWaitDuration;
        public int FoodWaitDuration => _foodWaitDuration;
        public float PatienceRecoveryAmount => _patienceRecoveryAmount;
        public List<int> RefreshCosts => _refreshCosts;
        public float TipRatio => _tipRatio;

        public int GetRefreshCost(int refreshCount)
        {
            if (refreshCount < 0 || refreshCount >= _refreshCosts.Count)
            {
                return _refreshCosts[_refreshCosts.Count - 1];
            }
            return _refreshCosts[refreshCount];
        }
    }
}
