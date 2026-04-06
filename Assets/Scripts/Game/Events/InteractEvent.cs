public readonly struct InteractEvent
{
    public IInteractable Target { get; }

    public InteractEvent(IInteractable target)
    {
        Target = target;
    }
}
