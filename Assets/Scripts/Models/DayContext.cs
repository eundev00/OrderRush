using UniRx;

namespace OrderRush.Models
{
    public class DayContext
    {
        public int DayNumber { get; set; }
        public ReactiveProperty<float> TimeBarElapsed { get; } = new(0f);
        public float TimeBarDuration { get; set; }
        public ReactiveProperty<int> EarnedCoins { get; } = new(0);
        public ReactiveProperty<int> SpawnedCustomers { get; } = new(0);
        public bool IsCompleted { get; set; }

    }
}
