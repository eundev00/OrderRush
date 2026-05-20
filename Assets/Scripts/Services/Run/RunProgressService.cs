using OrderRush.Data;
using OrderRush.Game;

namespace OrderRush.Services
{
    public class RunProgressService : IRunProgressService
    {
        private readonly RunsData _runsData;
        private int _currentRun;
        private DayContext _currentDayContext;
        private DaysData _currentDaysData;

        public int CurrentRun => _currentRun;
        public DayContext CurrentDayContext => _currentDayContext;
        public DaysData CurrentDaysData => _currentDaysData;

        public RunProgressService(RunsData runsData)
        {
            _runsData = runsData;
        }

        public void Initialize()
        {
            _currentRun = 1;
            _currentDaysData = _runsData.GetRun(_currentRun);
        }

        public void StartDay(int dayNumber)
        {
            _currentDayContext = new DayContext
            {
                DayNumber = dayNumber,
                TimeBarDuration = _currentDaysData.GetTimeBarDuration(dayNumber)
            };
        }

        public void CompleteDay()
        {
        }

        public void RestartDay()
        {
        }

        public void NextDay()
        {
        }

        public void CompleteRun()
        {
        }
    }
}
