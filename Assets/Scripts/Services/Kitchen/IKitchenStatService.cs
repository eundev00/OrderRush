public interface IKitchenStatService
{
    void AddDurationReduce(float reducePercent);
    float GetModifiedDuration();
    void AddSlowBurn(float extendPercent);
    float GetOvercookDuration();
}
