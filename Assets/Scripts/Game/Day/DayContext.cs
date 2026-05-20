namespace OrderRush.Game
{
    public class DayContext
    {
        public int DayNumber { get; set; }
        public float TimeBarElapsed { get; set; }
        public float TimeBarDuration { get; set; }
        public int EarnedCoins { get; set; }
        public int SpawnedCustomers { get; set; }
        public bool IsCompleted { get; set; }

        public float TimeBarProgress => TimeBarElapsed / TimeBarDuration;
        public bool IsTimeBarEnded => TimeBarElapsed >= TimeBarDuration;
    }
}
