using OrderRush.Data;
using OrderRush.Game;

namespace OrderRush.Services
{
    public interface IRunProgressService
    {
        int CurrentRun { get; }
        DayContext CurrentDayContext { get; }
        DaysData CurrentDaysData { get; }

        void Initialize();
        void StartDay(int dayNumber);
        void CompleteDay();
        void RestartDay();
        void NextDay();
        void CompleteRun();
    }
}
