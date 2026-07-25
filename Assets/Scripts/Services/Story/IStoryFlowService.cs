using Cysharp.Threading.Tasks;

public interface IStoryFlowService
{
    UniTask ShowStoryForDayAsync(int dayNumber);
}
