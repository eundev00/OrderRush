using System.Collections.Generic;
using UnityEngine;

namespace OrderRush.Data
{
    [CreateAssetMenu(fileName = "RunsData", menuName = "Order Rush/Day/RunsData", order = 100)]
    public class RunsData : ScriptableObject
    {
        [SerializeField] private List<DaysData> _runs = new();

        public List<DaysData> Runs => _runs;

        public DaysData GetRun(int runNumber)
        {
            return _runs.Find(run => run.RunNumber == runNumber);
        }

        public int GetTotalRunCount() => _runs.Count;
    }
}
