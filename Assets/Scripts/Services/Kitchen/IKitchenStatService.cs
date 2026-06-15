namespace OrderRush.Services
{
    public interface IKitchenStatService
    {
        void AddDurationReduce(float reducePercent);
        float GetModifiedDuration();
        void AddOvercookExtend(float extendPercent);
        float GetOvercookDuration();
    }
}
