using Cysharp.Threading.Tasks;
using OrderRush.Data;
using OrderRush.Models;

public interface IDayProgressService
{
    int CurrentRun { get; }
    DayContext CurrentDayContext { get; }
    DaysData CurrentDaysData { get; }

    UniTask Initialize();
    void InitDay(int dayNumber);
    void StartDayTimer();
    void CompleteDay();
    void FailDay();
    void RestartDay();
    void SetNextDay();
    void CompleteRun();
}
