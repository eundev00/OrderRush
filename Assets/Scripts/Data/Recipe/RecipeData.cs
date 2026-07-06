using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Order Rush/Recipe/Recipe", order = 51)]
public class RecipeData : ScriptableObject
{
    public int RecipeID;
    public string RecipeName;
    public Sprite Icon;
    public int SellPrice;
    public bool IsDefaultRecipe;
    public List<IngredientData> RequiredIngredients;



    public bool IsMatch(List<IngredientData> placedIngredients)
    {
        if (placedIngredients.Count != RequiredIngredients.Count)
            return false;

        foreach (var required in RequiredIngredients)
            if (!placedIngredients.Contains(required))
                return false;

        return true;
    }

}
