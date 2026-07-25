using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ShopService : IShopService
{
    private readonly IGameDataService _gameDataService;
    private readonly IAccountService _accountService;
    private readonly CardEffectApplier _effectApplier;

    public ShopService(
        IGameDataService gameDataService,
        IAccountService accountService,
        CardEffectApplier effectApplier)
    {
        _gameDataService = gameDataService;
        _accountService = accountService;
        _effectApplier = effectApplier;
    }

    // 슬롯0 = 테이블(노랑), 슬롯1·2 = 비테이블 후보에서 가중치 랜덤. 부족분은 SoldOut.
    public List<CardOffer> GetCurrentOffer()
    {
        var offers = new List<CardOffer>(3);
        offers.Add(MakeOffer(GetTableOfferCard()));

        var picks = PickWeighted(GetNonTableCandidates(), 2);
        for (int i = 0; i < 2; i++)
            offers.Add(i < picks.Count ? MakeOffer(picks[i]) : SoldOutOffer());

        return offers;
    }

    private int OwnedCount(int cardID)
    {
        var ids = _accountService.GetPurchasedCardIDs();
        int count = 0;
        for (int i = 0; i < ids.Count; i++)
            if (ids[i] == cardID) count++;
        return count;
    }

    private int GetPrice(CardData card) => card.Cost + card.PriceIncrement * OwnedCount(card.CardID);

    private CardColor GetColor(CardData card)
    {
        if (card.Effect != null && card.Effect.EffectType == EffectType.Table)
            return CardColor.Yellow;
        return OwnedCount(card.CardID) == 0 ? CardColor.Green : CardColor.Blue;
    }

    private CardOffer MakeOffer(CardData card)
    {
        if (card == null) return SoldOutOffer();
        return new CardOffer
        {
            Card = card,
            Price = GetPrice(card),
            Color = GetColor(card),
            SoldOut = false
        };
    }

    private CardOffer SoldOutOffer() => new CardOffer { SoldOut = true };

    private CardData FindLineCard(EffectType type)
    {
        var all = _gameDataService.GetAllCards();
        for (int i = 0; i < all.Count; i++)
            if (all[i].Effect != null && all[i].Effect.EffectType == type)
                return all[i];
        return null;
    }

    private CardData GetTableOfferCard()
    {
        var table = FindLineCard(EffectType.Table);
        if (table == null) return null;
        return OwnedCount(table.CardID) < table.MaxTier ? table : null;
    }

    private List<CardData> GetNonTableCandidates()
    {
        var result = new List<CardData>();
        var addedBuffLines = new HashSet<EffectType>(); // 같은 버프 라인은 후보에 1장만
        var all = _gameDataService.GetAllCards();
        for (int i = 0; i < all.Count; i++)
        {
            var c = all[i];
            if (c.Effect == null) continue;
            if (OwnedCount(c.CardID) >= c.MaxTier) continue;

            switch (c.Effect.EffectType)
            {
                case EffectType.CookSpeed:
                case EffectType.SlowBurn:
                    // 버프는 2장까지 허용하되 같은 라인 중복 방지(에셋이 여러 개여도 라인당 1장).
                    if (addedBuffLines.Add(c.Effect.EffectType))
                        result.Add(c);
                    break;
                case EffectType.Menu:
                    result.Add(c);
                    break;
            }
        }
        return result;
    }

    private List<CardData> PickWeighted(List<CardData> pool, int count)
    {
        var work = new List<CardData>(pool);
        var picked = new List<CardData>(count);

        for (int i = 0; i < count && work.Count > 0; i++)
        {
            int totalWeight = 0;
            for (int j = 0; j < work.Count; j++) totalWeight += work[j].Weight;

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            for (int j = 0; j < work.Count; j++)
            {
                cumulative += work[j].Weight;
                if (roll < cumulative)
                {
                    picked.Add(work[j]);
                    work.RemoveAt(j);
                    break;
                }
            }
        }
        return picked;
    }

    public async UniTask<bool> TryPurchaseCard(CardData card)
    {
        if (!_accountService.TrySpendCoins(GetPrice(card)))
            return false;

        int tier = OwnedCount(card.CardID) + 1;
        await _effectApplier.ApplyEffect(card.Effect, tier);

        // 원데이 전용(알바 등)은 소유되지 않으므로 구매기록에 남기지 않는다.
        if (!card.IsOneDayOnly)
            _accountService.AddPurchasedCard(card.CardID);

        return true;
    }
}
