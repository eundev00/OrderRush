/// <summary>
/// 런타임 재료 상태 관리
/// ScriptableObject인 IngredientData는 데이터 정의만, 상태 변경은 Context에서 관리
/// </summary>
public class IngredientContext : IHoldable
{
    public IngredientData Data { get; private set; }
    public IngredientState State { get; set; }

    public IngredientContext(IngredientData data)
    {
        Data = data;
        State = data.initialState;
    }

    public IngredientContext(IngredientData data, IngredientState state)
    {
        Data = data;
        State = state;
    }

    public override string ToString()
    {
        return $"{Data.ingredientName} ({State})";
    }
}
