using System;

[Serializable]
public class Order
{
    public RecipeData Recipe { get; private set; }
    public float TimeLimit { get; private set; }
    public bool IsCompleted { get; private set; }

    private float _timeRemaining;
    public float TimeRemaining => _timeRemaining;

    public Order(RecipeData recipe, float timeLimit)
    {
        Recipe = recipe;
        TimeLimit = timeLimit;
        _timeRemaining = timeLimit;
        IsCompleted = false;
    }

    public void UpdateTime(float deltaTime)
    {
        if (!IsCompleted)
        {
            _timeRemaining -= deltaTime;
        }
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    public bool IsExpired()
    {
        return _timeRemaining <= 0f && !IsCompleted;
    }
}
