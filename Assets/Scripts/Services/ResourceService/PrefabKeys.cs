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
    public const string BurntSteak = "BurntSteak";
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
    public const string Rain = "Rain";
    public const string Car1 = "Car1";
    public const string Car2 = "Car2";
    public const string Car3 = "Car3";
    public const string StreetLight = "StreetLight";
    public const string WalkerNpc1 = "WalkerNpc1";
    public const string WalkerNpc2 = "WalkerNpc2";
    public const string WalkerNpc3 = "WalkerNpc3";
    public const string HUD = "HUD";
    public const string Card = "Card";
    public const string PopupCardShop = "PopupCardShop";
    public const string PopupCompleted = "PopupCompleted";
    public const string PopupFailed = "PopupFailed";
    public const string PopupMessage = "PopupMessage";
    public const string PopupStory = "PopupStory";
    public const string CharacterEmoteIcon = "CharacterEmoteIcon";
    public const string CharacterOrderIcon = "CharacterOrderIcon";
    public const string FloatingCoinFX = "FloatingCoinFX";

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
        { BurntSteak, "Assets/Prefabs/Game/Ingredients/Steak/BurntSteak.prefab" },
        { Steak, "Assets/Prefabs/Game/Ingredients/Steak/Steak.prefab" },
        { SteakMeat, "Assets/Prefabs/Game/Ingredients/Steak/SteakMeat.prefab" },
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
        { Rain, "Assets/Prefabs/Game/Level/Rain.prefab" },
        { Car1, "Assets/Prefabs/Game/Street/Car1.prefab" },
        { Car2, "Assets/Prefabs/Game/Street/Car2.prefab" },
        { Car3, "Assets/Prefabs/Game/Street/Car3.prefab" },
        { StreetLight, "Assets/Prefabs/Game/Street/StreetLight.prefab" },
        { WalkerNpc1, "Assets/Prefabs/Game/Street/WalkerNpc1.prefab" },
        { WalkerNpc2, "Assets/Prefabs/Game/Street/WalkerNpc2.prefab" },
        { WalkerNpc3, "Assets/Prefabs/Game/Street/WalkerNpc3.prefab" },
        { HUD, "Assets/Prefabs/Game/UI/HUD.prefab" },
        { Card, "Assets/Prefabs/Game/UI/PopupCardShop/Card.prefab" },
        { PopupCardShop, "Assets/Prefabs/Game/UI/PopupCardShop/PopupCardShop.prefab" },
        { PopupCompleted, "Assets/Prefabs/Game/UI/PopupCompleted.prefab" },
        { PopupFailed, "Assets/Prefabs/Game/UI/PopupFailed.prefab" },
        { PopupMessage, "Assets/Prefabs/Game/UI/PopupMessage.prefab" },
        { PopupStory, "Assets/Prefabs/Game/UI/PopupStory.prefab" },
        { CharacterEmoteIcon, "Assets/Prefabs/Game/UI/WorldSpace/CharacterEmoteIcon.prefab" },
        { CharacterOrderIcon, "Assets/Prefabs/Game/UI/WorldSpace/CharacterOrderIcon.prefab" },
        { FloatingCoinFX, "Assets/Prefabs/Game/UI/WorldSpace/FloatingCoinFX.prefab" },
    };

    public static string GetPrefabPath(string tag)
    {
        if (PrefabPaths.TryGetValue(tag, out var path))
        {
             return path;
         }
         return string.Empty;
    }

    private const string LevelPathFormat = "Assets/Prefabs/Game/Level/LevelMap{0}.prefab";

    public static string GetLevelPath(int level) => string.Format(LevelPathFormat, level);
}
