public readonly struct DayCompletedData
{
    public readonly int DayNumber;
    public readonly int EarnedCoins;

    public DayCompletedData(int dayNumber, int earnedCoins)
    {
        DayNumber = dayNumber;
        EarnedCoins = earnedCoins;
    }
}
