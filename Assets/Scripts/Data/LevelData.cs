using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Order Rush/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    [SerializeField] private int _levelNumber;
    [SerializeField] private string _levelName;

    [Header("Recipes")]
    [SerializeField] private List<RecipeData> _availableRecipes;  // 이 레벨에서 주문 가능한 레시피

    [Header("Customer Settings")]
    [SerializeField] private float _customerSpawnInterval = 5f;  // 손님 스폰 간격 (초)
    [SerializeField] private int _maxCustomers = 3;  // 최대  손님 수
    [SerializeField] private int _groupCustomers = 1;

    [Header("Goal")]
    [SerializeField] private int _targetMoney;  // 목표 금액
    [SerializeField] private float _timeLimit = 180f;  // 제한 시간 (초), 0이면 무제한

    // Properties
    public int LevelNumber => _levelNumber;
    public string LevelName => _levelName;
    public List<RecipeData> AvailableRecipes => _availableRecipes;
    public float CustomerSpawnInterval => _customerSpawnInterval;
    public int MaxCustomers => _maxCustomers;
    public int TargetMoney => _targetMoney;
    public float TimeLimit => _timeLimit;
}
