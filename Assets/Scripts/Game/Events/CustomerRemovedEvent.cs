public readonly struct CustomerRemovedEvent
{
    public readonly bool WasServed;

    public CustomerRemovedEvent(bool wasServed)
    {
        WasServed = wasServed;
    }
}
