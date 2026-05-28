using System;
using System.Collections.Generic;

public static class DataKeys
{
    public const string Ingredient_GrilledMushroom = "Ingredient_GrilledMushroom";
    public const string Ingredient_Mushroom = "Ingredient_Mushroom";
    public const string Ingredient_Onion = "Ingredient_Onion";
    public const string Ingredient_OnionRing = "Ingredient_OnionRing";
    public const string Ingredient_SlicedOnion = "Ingredient_SlicedOnion";
    public const string Ingredient_Steak = "Ingredient_Steak";
    public const string Ingredient_SteakMeat = "Ingredient_SteakMeat";
    public const string Recipe_MushroomSteak = "Recipe_MushroomSteak";
    public const string Recipe_OnionRingSteak = "Recipe_OnionRingSteak";
    public const string Recipe_Steak = "Recipe_Steak";
    public const string Recipe_SteakPlatter = "Recipe_SteakPlatter";
    public const string GameConfig = "GameConfig";
    public const string Recipes = "Recipes";
    public const string Cards = "Cards";
    public const string Run1_Days = "Run1_Days";
    public const string RunsData = "RunsData";

    public static Dictionary<string, string> DataPaths = new Dictionary<string, string>()
    {
        { Ingredient_GrilledMushroom, "Assets/Data/Ingredient/Mushroom/GrilledMushroom.asset" },
        { Ingredient_Mushroom, "Assets/Data/Ingredient/Mushroom/Mushroom.asset" },
        { Ingredient_Onion, "Assets/Data/Ingredient/Onion/Onion.asset" },
        { Ingredient_OnionRing, "Assets/Data/Ingredient/Onion/OnionRing.asset" },
        { Ingredient_SlicedOnion, "Assets/Data/Ingredient/Onion/SlicedOnion.asset" },
        { Ingredient_Steak, "Assets/Data/Ingredient/Steak/Steak.asset" },
        { Ingredient_SteakMeat, "Assets/Data/Ingredient/Steak/SteakMeat.asset" },
        { Recipe_MushroomSteak, "Assets/Data/Recipe/MushroomSteak.asset" },
        { Recipe_OnionRingSteak, "Assets/Data/Recipe/OnionRingSteak.asset" },
        { Recipe_Steak, "Assets/Data/Recipe/Steak.asset" },
        { Recipe_SteakPlatter, "Assets/Data/Recipe/SteakPlatter.asset" },
        { GameConfig, "Assets/Data/GameConfig.asset" },
        { Recipes, "Assets/Data/Recipe/RecipesData.asset" },
        { Cards, "Assets/Data/Card/CardsData.asset" },
        { Run1_Days, "Assets/Data/Run1_Days.asset" },
        { RunsData, "Assets/Data/RunsData.asset" },
    };

    public static string GetDataPath(string tag)
    {
        if (DataPaths.TryGetValue(tag, out var path))
        {
            return path;
        }
        return string.Empty;
    }
}
