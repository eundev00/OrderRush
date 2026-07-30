using UniRx;

namespace OrderRush.Models
{
    public class DayContext
    {
        public ReactiveProperty<int> DayNumber { get; } = new(0);
        public ReactiveProperty<float> TimeBarElapsed { get; } = new(0f);
        public float TimeBarDuration { get; set; }
        public ReactiveProperty<int> EarnedCoins { get; } = new(0);
        public ReactiveProperty<int> SpawnedCustomers { get; } = new(0);
        public bool IsCompleted { get; set; }

        public void Reset()
        {
            DayNumber.Value = 0;
            TimeBarElapsed.Value = 0f;
            TimeBarDuration = 0f;
            EarnedCoins.Value = 0;
            SpawnedCustomers.Value = 0;
            IsCompleted = false;
        }
    }
}
