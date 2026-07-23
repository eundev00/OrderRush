using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class ShopService : IShopService
{
    private readonly IGameDataService _gameDataService;
    private readonly IAccountService _accountService;
    private readonly CardEffectApplier _effectApplier;
    private readonly List<int> _purchasedCardIDs = new();
    private int _refreshCount;

    public ShopService(
        IGameDataService gameDataService,
        IAccountService accountService,
        CardEffectApplier effectApplier)
    {
        _gameDataService = gameDataService;
        _accountService = accountService;
        _effectApplier = effectApplier;
    }

    public void Initialize()
    {
        _purchasedCardIDs.Clear();
        var savedIDs = _accountService.GetPurchasedCardIDs();
        _purchasedCardIDs.AddRange(savedIDs);
        _refreshCount = 0;
    }

    public List<CardData> GetRandomCardsForSelection(int count)
    {
        var allCards = _gameDataService.GetAllCards();
        var pool = new List<CardData>(allCards);
        var result = new List<CardData>(count);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int totalWeight = pool.Sum(c => c.Weight);
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;

            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += pool[j].Weight;
                if (roll < cumulative)
                {
                    result.Add(pool[j]);
                    pool.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
    }

    public int GetRefreshCost() => _gameDataService.GetRefreshCost(_refreshCount);

    public async UniTask<List<CardData>> Refresh()
    {
        int cost = GetRefreshCost();
        if (!_accountService.TrySpendCoins(cost))
            return null;

        _refreshCount++;
        return GetRandomCardsForSelection(3);
    }

    public async UniTask<bool> TryPurchaseCard(CardData card)
    {
        if (!_accountService.TrySpendCoins(card.Cost))
            return false;

        await _effectApplier.ApplyEffect(card.Effect);

        if (!card.IsExpiring)
        {
            _purchasedCardIDs.Add(card.CardID);
            _accountService.AddPurchasedCard(card.CardID);
        }

        return true;
    }
}
