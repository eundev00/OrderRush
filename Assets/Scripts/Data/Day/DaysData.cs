using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DaysData", menuName = "Order Rush/Day/DaysData", order = 100)]
public class DaysData : ScriptableObject
{
    [Header("Run Settings")]
    [SerializeField] private int _runNumber;
    [SerializeField] private int _rent;

    [Header("Difficulty Rules")]
    [SerializeField] private int _baseTimeBarDuration = 100;
    [SerializeField] private int _timeBarDurationIncrease = 25;
    [SerializeField] private int _baseCustomers = 4;
    [SerializeField] private int _customerIncrease = 1;
    [SerializeField] private int _daysInterval = 3;

    [Header("Story")]
    [SerializeField] private List<StoryDayData> _storyDays = new();

    public int GetTimeBarDuration(int dayNumber)
    {
        int intervalIndex = (dayNumber - 1) / _daysInterval;
        return _baseTimeBarDuration + (intervalIndex * _timeBarDurationIncrease);
    }

    public int GetMaxCustomers(int dayNumber)
    {
        int intervalIndex = (dayNumber - 1) / _daysInterval;
        return _baseCustomers + (intervalIndex * _customerIncrease);
    }

    public StoryDayData GetStoryForDay(int dayNumber)
    {
        for (int i = 0; i < _storyDays.Count; i++)
        {
            if (_storyDays[i].DayNumber == dayNumber)
            {
                return _storyDays[i];
            }
        }
        return null;
    }

    public int RunNumber => _runNumber;
    public int Rent => _rent;
}
