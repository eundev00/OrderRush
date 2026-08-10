using Cysharp.Threading.Tasks;

public interface IDayFlowService
{
    UniTask RunFirstDayAsync(int dayNumber);
}
