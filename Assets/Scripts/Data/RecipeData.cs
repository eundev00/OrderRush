using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Order Rush/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<IngredientData> ingredients = new();
    public List<CookingStep> steps = new();
    public IngredientData resultItem;  // 추가
}
