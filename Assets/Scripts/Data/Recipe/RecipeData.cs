using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Order Rush/Recipe")]
public class RecipeData : ScriptableObject
{
    public string RecipeName;
    public Sprite Icon;
    public List<IngredientData> RequiredIngredients;

    public bool IsComplete(List<IngredientData> placedIngredients)
    {
        if (placedIngredients.Count != RequiredIngredients.Count)
            return false;

        foreach (var required in RequiredIngredients)
            if (!placedIngredients.Contains(required))
                return false;

        return true;
    }
}
