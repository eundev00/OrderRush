using System.Collections.Generic;
using UnityEngine;

namespace OrderRush.Data
{
    [CreateAssetMenu(fileName = "DaysData", menuName = "Order Rush/DaysData")]
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
        [SerializeField] private List<StoryPhaseData> _storyPhases = new();

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

        public StoryPhaseData GetStoryPhaseData(int dayNumber)
        {
            for (int i = _storyPhases.Count - 1; i >= 0; i--)
            {
                if (dayNumber >= _storyPhases[i].DayStart)
                {
                    return _storyPhases[i];
                }
            }
            return null;
        }

        public int RunNumber => _runNumber;
        public int Rent => _rent;
    }
}
