public readonly struct PaymentEvent
{
    public readonly int Amount;
    public readonly string RecipeName;

    public PaymentEvent(int amount, string recipeName)
    {
        Amount = amount;
        RecipeName = recipeName;
    }
}
