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

        [Header("Customer Settings")]
        [SerializeField] private float _tipRatio = 0.1f;
        [SerializeField] private float _eatDuration = 5f;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnBufferDuration = 10f;
        [Range(0f, 1f)]
        [SerializeField] private float _defaultSpawnClusterStrength = 1f;

        [Header("Tool Settings")]
        [SerializeField] private float _toolProcessSeconds = 5f;

        public int EnterWaitDuration => _enterWaitDuration;
        public int OrderWaitDuration => _orderWaitDuration;
        public int FoodWaitDuration => _foodWaitDuration;
        public float PatienceRecoveryAmount => _patienceRecoveryAmount;
        public float TipRatio => _tipRatio;
        public float EatDuration => _eatDuration;
        public float SpawnBufferDuration => _spawnBufferDuration;
        public float DefaultSpawnClusterStrength => _defaultSpawnClusterStrength;
        public float ToolProcessSeconds => _toolProcessSeconds;
    }
}
