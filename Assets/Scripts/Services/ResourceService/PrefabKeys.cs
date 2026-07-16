using System;
using System.Collections.Generic;
   public static class PrefabKeys
   {
    public const string CustomerCharacter1 = "CustomerCharacter1";
    public const string CustomerCharacter2 = "CustomerCharacter2";
    public const string CustomerCharacter3 = "CustomerCharacter3";
    public const string CustomerCharacter4 = "CustomerCharacter4";
    public const string Player = "Player";
    public const string StaffServer1 = "StaffServer1";
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
    public const string Counter1 = "Counter1";
    public const string Counter2 = "Counter2";
    public const string KithenTableDeco = "KithenTableDeco";
    public const string ServingCounter = "ServingCounter";
    public const string SingleSink = "SingleSink";
    public const string Stove = "Stove";
    public const string TrashCan = "TrashCan";
    public const string LevelMap1 = "LevelMap1";
    public const string Card1 = "Card1";
    public const string Card2 = "Card2";
    public const string Card3 = "Card3";
    public const string PopupCardShop = "PopupCardShop";
    public const string PopupCompleted = "PopupCompleted";
    public const string CharacterEmoteIcon = "CharacterEmoteIcon";
    public const string CharacterOrderIcon = "CharacterOrderIcon";
    public const string FloatingCoinFX = "FloatingCoinFX";
    public const string KitchenGauge = "KitchenGauge";
    public const string TableGauge = "TableGauge";

    public static Dictionary<string, string> PrefabPaths = new Dictionary<string, string>()
    {
        { CustomerCharacter1, "Assets/Prefabs/Game/Character/CustomerCharacter1.prefab" },
        { CustomerCharacter2, "Assets/Prefabs/Game/Character/CustomerCharacter2.prefab" },
        { CustomerCharacter3, "Assets/Prefabs/Game/Character/CustomerCharacter3.prefab" },
        { CustomerCharacter4, "Assets/Prefabs/Game/Character/CustomerCharacter4.prefab" },
        { Player, "Assets/Prefabs/Game/Character/Player.prefab" },
        { StaffServer1, "Assets/Prefabs/Game/Character/StaffServer1.prefab" },
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
        { Counter1, "Assets/Prefabs/Game/Kitchen/Tools/Counter1.prefab" },
        { Counter2, "Assets/Prefabs/Game/Kitchen/Tools/Counter2.prefab" },
        { KithenTableDeco, "Assets/Prefabs/Game/Kitchen/Tools/KithenTableDeco.prefab" },
        { ServingCounter, "Assets/Prefabs/Game/Kitchen/Tools/ServingCounter.prefab" },
        { SingleSink, "Assets/Prefabs/Game/Kitchen/Tools/SingleSink.prefab" },
        { Stove, "Assets/Prefabs/Game/Kitchen/Tools/Stove.prefab" },
        { TrashCan, "Assets/Prefabs/Game/Kitchen/Tools/TrashCan.prefab" },
        { LevelMap1, "Assets/Prefabs/Game/Level/LevelMap1.prefab" },
        { Card1, "Assets/Prefabs/Game/UI/PopupCardShop/Card1.prefab" },
        { Card2, "Assets/Prefabs/Game/UI/PopupCardShop/Card2.prefab" },
        { Card3, "Assets/Prefabs/Game/UI/PopupCardShop/Card3.prefab" },
        { PopupCardShop, "Assets/Prefabs/Game/UI/PopupCardShop/PopupCardShop.prefab" },
        { CharacterEmoteIcon, "Assets/Prefabs/Game/UI/WorldSpace/CharacterEmoteIcon.prefab" },
        { CharacterOrderIcon, "Assets/Prefabs/Game/UI/WorldSpace/CharacterOrderIcon.prefab" },
        { FloatingCoinFX, "Assets/Prefabs/Game/UI/WorldSpace/FloatingCoinFX.prefab" },
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
