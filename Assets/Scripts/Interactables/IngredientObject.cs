using UnityEngine;

/// <summary>
/// 월드에 존재하는 재료 오브젝트
/// IngredientData ScriptableObject 데이터를 참조하고 런타임 상태를 관리
/// </summary>
public class IngredientObject : MonoBehaviour, IHoldable
{
    public IngredientContext Context { get; private set; }


    public void Initialize(IngredientData ingredient)
    {
        Context = new IngredientContext(ingredient);
    }
}
