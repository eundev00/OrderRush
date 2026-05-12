public readonly struct TableAvailableEvent
{
    public readonly DiningTable Table;

    public TableAvailableEvent(DiningTable table)
    {
        Table = table;
    }
}
