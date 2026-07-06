using System;
using System.Collections.Generic;

public static class DataKeys
{
    public const string ServedStaffEffect = "ServedStaffEffect";
    public const string SlowBrunEffect10 = "SlowBrunEffect10";
    public const string SlowBurnEffect20 = "SlowBurnEffect20";
    public const string TableEffect = "TableEffect";
    public const string UpgradeCardEffect10 = "UpgradeCardEffect10";
    public const string UpgradeCardEffect20 = "UpgradeCardEffect20";
    public const string CardsData = "CardsData";
    public const string SlowBurnCard_Grade1 = "SlowBurnCard_Grade1";
    public const string SlowBurnCard_Grade2 = "SlowBurnCard_Grade2";
    public const string StaffCard_Grade1 = "StaffCard_Grade1";
    public const string TableCard = "TableCard";
    public const string UpgradCardData_Grade1 = "UpgradCardData_Grade1";
    public const string UpgradCardData_Grade2 = "UpgradCardData_Grade2";
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
        { ServedStaffEffect, "ServedStaffEffect" },
        { SlowBrunEffect10, "SlowBrunEffect10" },
        { SlowBurnEffect20, "SlowBurnEffect20" },
        { TableEffect, "TableEffect" },
        { UpgradeCardEffect10, "UpgradeCardEffect10" },
        { UpgradeCardEffect20, "UpgradeCardEffect20" },
        { CardsData, "CardsData" },
        { SlowBurnCard_Grade1, "SlowBurnCard_Grade1" },
        { SlowBurnCard_Grade2, "SlowBurnCard_Grade2" },
        { StaffCard_Grade1, "StaffCard_Grade1" },
        { TableCard, "TableCard" },
        { UpgradCardData_Grade1, "UpgradCardData_Grade1" },
        { UpgradCardData_Grade2, "UpgradCardData_Grade2" },
        { GameConfig, "GameConfig" },
        { GrilledMushroom, "GrilledMushroom" },
        { Mushroom, "Mushroom" },
        { Onion, "Onion" },
        { OnionRing, "OnionRing" },
        { SlicedOnion, "SlicedOnion" },
        { Steak, "Steak" },
        { SteakMeat, "SteakMeat" },
        { Recipe01_Steak, "Recipe01_Steak" },
        { Recipe02_OnionRingSteak, "Recipe02_OnionRingSteak" },
        { RecipesData, "RecipesData" },
        { Run1_Days, "Run1_Days" },
        { RunsData, "RunsData" },
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
