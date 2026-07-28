using Cysharp.Threading.Tasks;

public interface IDayFlowService
{
    UniTask RunFirstDayAsync(int dayNumber);
    UniTask HandleDayEndedAsync(DayEndedEvent evt);
}
