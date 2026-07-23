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
        foreach (var id in purchasedIDs)
        {
            var card = _gameDataService.GetCardByID(id);
            if (card != null)
            {
                await ApplyEffect(card.Effect);
            }
        }
    }

    public async UniTask ApplyEffect(CardEffectData effect)
    {
        switch (effect.EffectType)
        {
            case EffectType.Table:
                await _levelPresenter.AddTableFromEffect((TableAdditionEffect)effect);
                break;
            case EffectType.Menu:
                ApplyMenuUnlock((MenuEffect)effect);
                break;
            case EffectType.ToolUpgrade:
                ApplyUpgrade((UpgradeEffect)effect);
                break;
            case EffectType.StaffHire:
                await ApplyStaffHire((StaffEffect)effect);
                break;
            case EffectType.SlowBurn:
                ApplySlowBurnExtend((SlowBurnEffect)effect);
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

    private void ApplyUpgrade(UpgradeEffect effect)
    {
        _kitchenStatService.AddDurationReduce(effect.DurationReducePercent);
    }

    private void ApplySlowBurnExtend(SlowBurnEffect effect)
    {
        _kitchenStatService.AddSlowBurn(effect.ExtendPercent);
    }
}
