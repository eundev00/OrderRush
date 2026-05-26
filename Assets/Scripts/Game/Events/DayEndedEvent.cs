public readonly struct DayEndedEvent
{
    public readonly int NextDay;

    public DayEndedEvent(int nextDay)
    {
        NextDay = nextDay;
    }
}
