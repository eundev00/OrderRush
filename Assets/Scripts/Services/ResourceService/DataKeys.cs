using System;
using System.Collections.Generic;

public static class DataKeys
{
    public const string TableEffect = "TableEffect";
    public const string CardsData = "CardsData";
    public const string TableCard = "TableCard";
    public const string GameConfig = "GameConfig";
    public const string Ingredient_GrilledMushroom = "Ingredient_GrilledMushroom";
    public const string Ingredient_Mushroom = "Ingredient_Mushroom";
    public const string Ingredient_Onion = "Ingredient_Onion";
    public const string Ingredient_OnionRing = "Ingredient_OnionRing";
    public const string Ingredient_SlicedOnion = "Ingredient_SlicedOnion";
    public const string Ingredient_Steak = "Ingredient_Steak";
    public const string Ingredient_SteakMeat = "Ingredient_SteakMeat";
    public const string Recipe_Recipe01_Steak = "Recipe_Recipe01_Steak";
    public const string Recipe_Recipe02_OnionRingSteak = "Recipe_Recipe02_OnionRingSteak";
    public const string Recipe_RecipesData = "Recipe_RecipesData";
    public const string Run1_Days = "Run1_Days";
    public const string RunsData = "RunsData";

    public static Dictionary<string, string> DataPaths = new Dictionary<string, string>()
    {
        { TableEffect, "Assets/Data/Card/CardEffect/TableEffect.asset" },
        { CardsData, "Assets/Data/Card/CardsData.asset" },
        { TableCard, "Assets/Data/Card/TableCard.asset" },
        { GameConfig, "Assets/Data/GameConfig.asset" },
        { Ingredient_GrilledMushroom, "Assets/Data/Ingredient/Mushroom/GrilledMushroom.asset" },
        { Ingredient_Mushroom, "Assets/Data/Ingredient/Mushroom/Mushroom.asset" },
        { Ingredient_Onion, "Assets/Data/Ingredient/Onion/Onion.asset" },
        { Ingredient_OnionRing, "Assets/Data/Ingredient/Onion/OnionRing.asset" },
        { Ingredient_SlicedOnion, "Assets/Data/Ingredient/Onion/SlicedOnion.asset" },
        { Ingredient_Steak, "Assets/Data/Ingredient/Steak/Steak.asset" },
        { Ingredient_SteakMeat, "Assets/Data/Ingredient/Steak/SteakMeat.asset" },
        { Recipe_Recipe01_Steak, "Assets/Data/Recipe/Recipe01_Steak.asset" },
        { Recipe_Recipe02_OnionRingSteak, "Assets/Data/Recipe/Recipe02_OnionRingSteak.asset" },
        { Recipe_RecipesData, "Assets/Data/Recipe/RecipesData.asset" },
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
