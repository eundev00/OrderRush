using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class CardEffectApplier
{
    private readonly ILevelContextPresenter _levelPresenter;
    private readonly SpawnFactory _spawnFactory;
    private readonly IAccountService _accountService;
    private readonly IKitchenStatService _kitchenStatService;
    private readonly IGameDataService _gameDataService;

    public CardEffectApplier(
        ILevelContextPresenter levelPresenter,
        SpawnFactory spawnFactory,
        IAccountService accountService,
        IKitchenStatService kitchenStatService,
        IGameDataService gameDataService)
    {
        _levelPresenter = levelPresenter;
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
                await _levelPresenter.AddTableFromEffect((TableAdditionEffect)effect);
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

    private async UniTask ApplyStaffHire(StaffEffect effect)
    {
        var staff = await _spawnFactory.Create<StaffCharacter>(effect.StaffPrefabName);
        if (staff == null) return;

        staff.WarpTo(_levelPresenter.SpawnPosition);
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
