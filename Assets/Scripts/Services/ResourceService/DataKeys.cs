using System;
using System.Collections.Generic;

public static class DataKeys
{
    public const string SlowBrunEffect10 = "SlowBrunEffect10";
    public const string SlowBurnEffect20 = "SlowBurnEffect20";
    public const string StaffEffectServer1 = "StaffEffectServer1";
    public const string TableEffect = "TableEffect";
    public const string UpgradeCardEffect10 = "UpgradeCardEffect10";
    public const string UpgradeCardEffect20 = "UpgradeCardEffect20";
    public const string CardsData = "CardsData";
    public const string SlowBurnCard_Grade1 = "SlowBurnCard_Grade1";
    public const string SlowBurnCard_Grade2 = "SlowBurnCard_Grade2";
    public const string StaffCard_Server1 = "StaffCard_Server1";
    public const string TableCard = "TableCard";
    public const string UpgradCardData_Grade1 = "UpgradCardData_Grade1";
    public const string UpgradCardData_Grade2 = "UpgradCardData_Grade2";
    public const string CustomerTrait_Normal = "CustomerTrait_Normal";
    public const string CustomerTrait_Tipper = "CustomerTrait_Tipper";
    public const string DayNightSettings = "DayNightSettings";
    public const string GameConfig = "GameConfig";
    public const string GrilledMushroom = "GrilledMushroom";
    public const string Mushroom = "Mushroom";
    public const string Onion = "Onion";
    public const string OnionRing = "OnionRing";
    public const string SlicedOnion = "SlicedOnion";
    public const string Steak = "Steak";
    public const string SteakMeat = "SteakMeat";
    public const string Recipe01_Steak = "Recipe01_Steak";
    public const string Recipe02_OnionRingSteak = "Recipe02_OnionRingSteak";
    public const string RecipesData = "RecipesData";
    public const string Run1_Days = "Run1_Days";
    public const string RunsData = "RunsData";

    public static Dictionary<string, string> DataPaths = new Dictionary<string, string>()
    {
        { SlowBrunEffect10, "Assets/Data/Card/CardEffect/SlowBrunEffect10.asset" },
        { SlowBurnEffect20, "Assets/Data/Card/CardEffect/SlowBurnEffect20.asset" },
        { StaffEffectServer1, "Assets/Data/Card/CardEffect/StaffEffectServer1.asset" },
        { TableEffect, "Assets/Data/Card/CardEffect/TableEffect.asset" },
        { UpgradeCardEffect10, "Assets/Data/Card/CardEffect/UpgradeCardEffect10.asset" },
        { UpgradeCardEffect20, "Assets/Data/Card/CardEffect/UpgradeCardEffect20.asset" },
        { CardsData, "Assets/Data/Card/CardsData.asset" },
        { SlowBurnCard_Grade1, "Assets/Data/Card/SlowBurnCard_Grade1.asset" },
        { SlowBurnCard_Grade2, "Assets/Data/Card/SlowBurnCard_Grade2.asset" },
        { StaffCard_Server1, "Assets/Data/Card/StaffCard_Server1.asset" },
        { TableCard, "Assets/Data/Card/TableCard.asset" },
        { UpgradCardData_Grade1, "Assets/Data/Card/UpgradCardData_Grade1.asset" },
        { UpgradCardData_Grade2, "Assets/Data/Card/UpgradCardData_Grade2.asset" },
        { CustomerTrait_Normal, "Assets/Data/CustomerTrait/CustomerTrait_Normal.asset" },
        { CustomerTrait_Tipper, "Assets/Data/CustomerTrait/CustomerTrait_Tipper.asset" },
        { DayNightSettings, "Assets/Data/DayNightSettings.asset" },
        { GameConfig, "Assets/Data/GameConfig.asset" },
        { GrilledMushroom, "Assets/Data/Ingredient/Mushroom/GrilledMushroom.asset" },
        { Mushroom, "Assets/Data/Ingredient/Mushroom/Mushroom.asset" },
        { Onion, "Assets/Data/Ingredient/Onion/Onion.asset" },
        { OnionRing, "Assets/Data/Ingredient/Onion/OnionRing.asset" },
        { SlicedOnion, "Assets/Data/Ingredient/Onion/SlicedOnion.asset" },
        { Steak, "Assets/Data/Ingredient/Steak/Steak.asset" },
        { SteakMeat, "Assets/Data/Ingredient/Steak/SteakMeat.asset" },
        { Recipe01_Steak, "Assets/Data/Recipe/Recipe01_Steak.asset" },
        { Recipe02_OnionRingSteak, "Assets/Data/Recipe/Recipe02_OnionRingSteak.asset" },
        { RecipesData, "Assets/Data/Recipe/RecipesData.asset" },
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
