using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipesData", menuName = "Order Rush/Recipe/Recipes Data", order = 51)]
public class RecipesData : ScriptableObject
{
    [SerializeField] private List<RecipeData> _recipes = new();

    public List<RecipeData> Recipes => _recipes;
}
