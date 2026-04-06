using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsData", menuName = "Order Rush/LevelsData")]
public class LevelsData : ScriptableObject
{
    [SerializeField] private List<LevelData> _levels = new();

    public List<LevelData> Levels => _levels;

    public LevelData GetLevel(int levelNumber)
    {
        return _levels.Find(level => level.LevelNumber == levelNumber);
    }

    public int GetTotalLevelCount() => _levels.Count;
}
