using System;
using System.Collections.Generic;

public static class DataKeys
{
    public const string CookSpeedCardEffect = "CookSpeedCardEffect";
    public const string MoveSpeedCardEffect = "MoveSpeedCardEffect";
    public const string SlowBrunEffect = "SlowBrunEffect";
    public const string StaffEffectServer1 = "StaffEffectServer1";
    public const string TableEffect = "TableEffect";
    public const string CardsData = "CardsData";
    public const string CookSpeedCard = "CookSpeedCard";
    public const string MoveSpeedCard = "MoveSpeedCard";
    public const string SlowBurnCard = "SlowBurnCard";
    public const string TableCard = "TableCard";
    public const string CustomerTrait_Normal = "CustomerTrait_Normal";
    public const string CustomerTrait_Tipper = "CustomerTrait_Tipper";
    public const string DayNightSettings = "DayNightSettings";
    public const string GameConfig = "GameConfig";
    public const string GrilledMushroom = "GrilledMushroom";
    public const string Mushroom = "Mushroom";
    public const string Onion = "Onion";
    public const string OnionRing = "OnionRing";
    public const string SlicedOnion = "SlicedOnion";
    public const string BurntSteak = "BurntSteak";
    public const string Steak = "Steak";
    public const string SteakMeat = "SteakMeat";
    public const string Recipe01_Steak = "Recipe01_Steak";
    public const string Recipe02_OnionRingSteak = "Recipe02_OnionRingSteak";
    public const string RecipesData = "RecipesData";
    public const string Run1_Days = "Run1_Days";
    public const string RunsData = "RunsData";
    public const string StreetTrafficData = "StreetTrafficData";

    public static Dictionary<string, string> DataPaths = new Dictionary<string, string>()
    {
        { CookSpeedCardEffect, "Assets/Data/Card/CardEffect/CookSpeedCardEffect.asset" },
        { MoveSpeedCardEffect, "Assets/Data/Card/CardEffect/MoveSpeedCardEffect.asset" },
        { SlowBrunEffect, "Assets/Data/Card/CardEffect/SlowBrunEffect.asset" },
        { StaffEffectServer1, "Assets/Data/Card/CardEffect/StaffEffectServer1.asset" },
        { TableEffect, "Assets/Data/Card/CardEffect/TableEffect.asset" },
        { CardsData, "Assets/Data/Card/CardsData.asset" },
        { CookSpeedCard, "Assets/Data/Card/CookSpeedCard.asset" },
        { MoveSpeedCard, "Assets/Data/Card/MoveSpeedCard.asset" },
        { SlowBurnCard, "Assets/Data/Card/SlowBurnCard.asset" },
        { TableCard, "Assets/Data/Card/TableCard.asset" },
        { CustomerTrait_Normal, "Assets/Data/CustomerTrait/CustomerTrait_Normal.asset" },
        { CustomerTrait_Tipper, "Assets/Data/CustomerTrait/CustomerTrait_Tipper.asset" },
        { DayNightSettings, "Assets/Data/DayNightSettings.asset" },
        { GameConfig, "Assets/Data/GameConfig.asset" },
        { GrilledMushroom, "Assets/Data/Ingredient/Mushroom/GrilledMushroom.asset" },
        { Mushroom, "Assets/Data/Ingredient/Mushroom/Mushroom.asset" },
        { Onion, "Assets/Data/Ingredient/Onion/Onion.asset" },
        { OnionRing, "Assets/Data/Ingredient/Onion/OnionRing.asset" },
        { SlicedOnion, "Assets/Data/Ingredient/Onion/SlicedOnion.asset" },
        { BurntSteak, "Assets/Data/Ingredient/Steak/BurntSteak.asset" },
        { Steak, "Assets/Data/Ingredient/Steak/Steak.asset" },
        { SteakMeat, "Assets/Data/Ingredient/Steak/SteakMeat.asset" },
        { Recipe01_Steak, "Assets/Data/Recipe/Recipe01_Steak.asset" },
        { Recipe02_OnionRingSteak, "Assets/Data/Recipe/Recipe02_OnionRingSteak.asset" },
        { RecipesData, "Assets/Data/Recipe/RecipesData.asset" },
        { Run1_Days, "Assets/Data/Run1_Days.asset" },
        { RunsData, "Assets/Data/RunsData.asset" },
        { StreetTrafficData, "Assets/Data/StreetTrafficData.asset" },
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
