public readonly struct DayEndedEvent
{
    public readonly int NextDay;
    public readonly bool IsCompleted;
    public readonly int EarnedCoins;

    public DayEndedEvent(int nextDay, bool isCompleted, int earnedCoins)
    {
        NextDay = nextDay;
        IsCompleted = isCompleted;
        EarnedCoins = earnedCoins;
    }
}
