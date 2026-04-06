using System;
using System.Collections.Generic;
   public static class PrefabKeys
   {
    public const string DefaultCube = "DefaultCube";
    public const string DiningChair = "DiningChair";
    public const string DiningTable2 = "DiningTable2";
    public const string Steak = "Steak";
    public const string SteakMeat = "SteakMeat";
    public const string Plate = "Plate";
    public const string PlateRack = "PlateRack";
    public const string Player = "Player";
    public const string MeatFridge = "MeatFridge";
    public const string KithenTable = "KithenTable";
    public const string Stove = "Stove";
    public const string TrashCan = "TrashCan";
    public const string ProjectLifetimeScope = "ProjectLifetimeScope";
    public const string Stage1 = "Stage1";
    public const string TestCube = "TestCube";

    public static Dictionary<string, string> PrefabPaths = new Dictionary<string, string>()
    {
        { DefaultCube, "Assets/Prefabs/Game/DefaultCube.prefab" },
        { DiningChair, "Assets/Prefabs/Game/Dining/DiningChair.prefab" },
        { DiningTable2, "Assets/Prefabs/Game/Dining/DiningTable2.prefab" },
        { Steak, "Assets/Prefabs/Game/Ingredients/Steak.prefab" },
        { SteakMeat, "Assets/Prefabs/Game/Ingredients/SteakMeat.prefab" },
        { Plate, "Assets/Prefabs/Game/Kitchen/Plate/Plate.prefab" },
        { PlateRack, "Assets/Prefabs/Game/Kitchen/Plate/PlateRack.prefab" },
        { Player, "Assets/Prefabs/Game/Kitchen/Player.prefab" },
        { MeatFridge, "Assets/Prefabs/Game/Kitchen/Storage/MeatFridge.prefab" },
        { KithenTable, "Assets/Prefabs/Game/Kitchen/Tools/KithenTable.prefab" },
        { Stove, "Assets/Prefabs/Game/Kitchen/Tools/Stove.prefab" },
        { TrashCan, "Assets/Prefabs/Game/Kitchen/Tools/TrashCan.prefab" },
        { ProjectLifetimeScope, "Assets/Prefabs/Game/ProjectLifetimeScope.prefab" },
        { Stage1, "Assets/Prefabs/Game/Stage/Stage1.prefab" },
        { TestCube, "Assets/Prefabs/Game/TestCube.prefab" },
    };

    public static string GetPrefabPath(string tag)
    {
        if (PrefabPaths.TryGetValue(tag, out var path))
        {
             return path;
         }
         return string.Empty;
    }
}
