using UnityEngine;

public class CustomerCharacter : CharacterBase
{
    public Order Order { get; private set; }
    public DiningSeat AssignedSeat { get; private set; }

    private const float DEFAULT_TIME_LIMIT = 60f;

    public void GoToSeat(DiningSeat targetSeat, RecipeData recipe)
    {
        AssignedSeat = targetSeat;

        // 이동 → 착석 → 주문 생성
        EnqueueAction(new MoveAction(_mover, targetSeat.SitPoint.position));
        EnqueueAction(new SitAction(this, targetSeat));
        EnqueueAction(new OrderAction(this, recipe));
    }

    public void CreateOrder(RecipeData recipe)
    {
        Order = new Order(recipe, DEFAULT_TIME_LIMIT);
        Debug.Log($"[CustomerCharacter] Order created: {recipe.RecipeName} (Time limit: {DEFAULT_TIME_LIMIT}s)");
    }
}
