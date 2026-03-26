using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Order Rush/Ingredient")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;
    public IngredientState initialState;
    public Sprite icon;
    public GameObject prefab;
}
