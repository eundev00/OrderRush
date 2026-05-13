using System;
using System.Collections.Generic;
   public static class PrefabKeys
   {
    public const string CustomerCharacter1 = "CustomerCharacter1";
    public const string CustomerCharacter2 = "CustomerCharacter2";
    public const string CustomerCharacter3 = "CustomerCharacter3";
    public const string CustomerCharacter4 = "CustomerCharacter4";
    public const string Player = "Player";
    public const string DefaultCube = "DefaultCube";
    public const string DiningChair = "DiningChair";
    public const string DiningTable2 = "DiningTable2";
    public const string GameObject = "GameObject";
    public const string Steak = "Steak";
    public const string SteakMeat = "SteakMeat";
    public const string Plate = "Plate";
    public const string PlateRack = "PlateRack";
    public const string FoodStorage = "FoodStorage";
    public const string Refrigerator = "Refrigerator";
    public const string DoubleSink = "DoubleSink";
    public const string KithenTable = "KithenTable";
    public const string KithenTable_Door = "KithenTable_Door";
    public const string SingleSink = "SingleSink";
    public const string Stove = "Stove";
    public const string TrashCan = "TrashCan";
    public const string LevelMap1 = "LevelMap1";
    public const string CharacterEmoteIcon = "CharacterEmoteIcon";
    public const string CharacterOrderIcon = "CharacterOrderIcon";
    public const string KitchenGauge = "KitchenGauge";
    public const string TableGauge = "TableGauge";

    public static Dictionary<string, string> PrefabPaths = new Dictionary<string, string>()
    {
        { CustomerCharacter1, "Assets/Prefabs/Game/Character/CustomerCharacter1.prefab" },
        { CustomerCharacter2, "Assets/Prefabs/Game/Character/CustomerCharacter2.prefab" },
        { CustomerCharacter3, "Assets/Prefabs/Game/Character/CustomerCharacter3.prefab" },
        { CustomerCharacter4, "Assets/Prefabs/Game/Character/CustomerCharacter4.prefab" },
        { Player, "Assets/Prefabs/Game/Character/Player.prefab" },
        { DefaultCube, "Assets/Prefabs/Game/DefaultCube.prefab" },
        { DiningChair, "Assets/Prefabs/Game/Dining/DiningChair.prefab" },
        { DiningTable2, "Assets/Prefabs/Game/Dining/DiningTable2.prefab" },
        { GameObject, "Assets/Prefabs/Game/GameObject.prefab" },
        { Steak, "Assets/Prefabs/Game/Ingredients/Steak.prefab" },
        { SteakMeat, "Assets/Prefabs/Game/Ingredients/SteakMeat.prefab" },
        { Plate, "Assets/Prefabs/Game/Kitchen/Plate/Plate.prefab" },
        { PlateRack, "Assets/Prefabs/Game/Kitchen/Plate/PlateRack.prefab" },
        { FoodStorage, "Assets/Prefabs/Game/Kitchen/Storage/FoodStorage.prefab" },
        { Refrigerator, "Assets/Prefabs/Game/Kitchen/Storage/Refrigerator.prefab" },
        { DoubleSink, "Assets/Prefabs/Game/Kitchen/Tools/DoubleSink.prefab" },
        { KithenTable, "Assets/Prefabs/Game/Kitchen/Tools/KithenTable.prefab" },
        { KithenTable_Door, "Assets/Prefabs/Game/Kitchen/Tools/KithenTable_Door.prefab" },
        { SingleSink, "Assets/Prefabs/Game/Kitchen/Tools/SingleSink.prefab" },
        { Stove, "Assets/Prefabs/Game/Kitchen/Tools/Stove.prefab" },
        { TrashCan, "Assets/Prefabs/Game/Kitchen/Tools/TrashCan.prefab" },
        { LevelMap1, "Assets/Prefabs/Game/Level/LevelMap1.prefab" },
        { CharacterEmoteIcon, "Assets/Prefabs/Game/UI/WorldSpace/CharacterEmoteIcon.prefab" },
        { CharacterOrderIcon, "Assets/Prefabs/Game/UI/WorldSpace/CharacterOrderIcon.prefab" },
        { KitchenGauge, "Assets/Prefabs/Game/UI/WorldSpace/KitchenGauge.prefab" },
        { TableGauge, "Assets/Prefabs/Game/UI/WorldSpace/TableGauge.prefab" },
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
