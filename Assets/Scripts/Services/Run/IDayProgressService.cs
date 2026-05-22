using Cysharp.Threading.Tasks;
using OrderRush.Data;
using OrderRush.Models;

namespace OrderRush.Services
{
    public interface IDayProgressService
    {
        int CurrentRun { get; }
        DayContext CurrentDayContext { get; }
        DaysData CurrentDaysData { get; }

        UniTask Initialize();
        void StartDay(int dayNumber);
        void CompleteDay();
        void FailDay();
        void RestartDay();
        void NextDay();
        void CompleteRun();
    }
}
