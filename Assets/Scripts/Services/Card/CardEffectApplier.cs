using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardEffectApplier
{
    private readonly ILevelService _levelService;
    private readonly SpawnFactory _spawnFactory;
    private readonly IAccountService _accountService;
    private readonly IKitchenStatService _kitchenStatService;
    private readonly IGameDataService _gameDataService;

    public CardEffectApplier(
        ILevelService levelService,
        SpawnFactory spawnFactory,
        IAccountService accountService,
        IKitchenStatService kitchenStatService,
        IGameDataService gameDataService)
    {
        _levelService = levelService;
        _spawnFactory = spawnFactory;
        _accountService = accountService;
        _kitchenStatService = kitchenStatService;
        _gameDataService = gameDataService;
    }

    public async UniTask ApplyAllPurchasedCards()
    {
        var purchasedIDs = _accountService.GetPurchasedCardIDs();
        var tierCounts = new Dictionary<int, int>();

        foreach (var id in purchasedIDs)
        {
            if (!tierCounts.ContainsKey(id))
                tierCounts[id] = 0;
            tierCounts[id]++;
        }

        foreach (var kvp in tierCounts)
        {
            var card = _gameDataService.GetCardByID(kvp.Key);
            if (card != null)
            {
                await ApplyEffect(card.Effect, kvp.Value);
            }
        }
    }

    public async UniTask ApplyEffect(CardEffectData effect, int tier = 1)
    {
        switch (effect.EffectType)
        {
            case EffectType.Table:
                await ApplyTableAddition((TableAdditionEffect)effect, tier);
                break;
            case EffectType.Menu:
                ApplyMenuUnlock((MenuEffect)effect);
                break;
            case EffectType.CookSpeed:
                ApplyUpgrade((UpgradeEffect)effect, tier);
                break;
            case EffectType.StaffHire:
                await ApplyStaffHire((StaffEffect)effect);
                break;
            case EffectType.SlowBurn:
                ApplySlowBurnExtend((SlowBurnEffect)effect, tier);
                break;
        }
    }

    private async UniTask ApplyTableAddition(TableAdditionEffect effect, int targetCount)
    {
        var context = _levelService.Context;
        int currentCount = context.DiningTables.Count;

        for (int i = currentCount; i < targetCount; i++)
        {
            Transform spawnPoint = context.GetNextTableSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("No more table spawn points available");
                return;
            }

            var table = await _spawnFactory.Create<DiningTable>(
                effect.TablePrefabName,
                spawnPoint.position,
                spawnPoint);

            if (table != null)
            {
                table.transform.rotation = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
                context.AddDiningTable(table);
            }
        }
    }

    private async UniTask ApplyStaffHire(StaffEffect effect)
    {
        var staff = await _spawnFactory.Create<StaffCharacter>(effect.StaffPrefabName);
        if (staff == null) return;

        staff.WarpTo(_levelService.Context.SpawnPoint.position);
    }

    private void ApplyMenuUnlock(MenuEffect effect)
    {
        _accountService.AddOwnedRecipe(effect.Recipe.RecipeID);
    }

    private void ApplyUpgrade(UpgradeEffect effect, int tier)
    {
        float value = effect.BaseDurationReducePercent + (tier - 1) * effect.DurationReducePercentPerTier;
        _kitchenStatService.AddDurationReduce(value);
    }

    private void ApplySlowBurnExtend(SlowBurnEffect effect, int tier)
    {
        float value = effect.BaseExtendPercent + (tier - 1) * effect.ExtendPercentPerTier;
        _kitchenStatService.AddSlowBurn(value);
    }
}
