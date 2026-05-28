using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace OrderRush.Services
{
    public class CardService : ICardService
    {
        private readonly IGameDataService _gameDataService;
        private readonly IAccountService _accountService;
        private readonly CardEffectApplier _effectApplier;
        private readonly List<int> _purchasedCardIDs = new();

        public CardService(
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
        }

        public List<CardData> GetRandomCardsForSelection(int count)
        {
            var excludeIDs = _purchasedCardIDs
                .Where(id =>
                {
                    var card = _gameDataService.Cards.GetCard(id);
                    return card != null && card.IsExpiring;
                })
                .ToList();

            return _gameDataService.Cards.GetRandomCards(count, excludeIDs);
        }

        public async UniTask<bool> TryPurchaseCard(CardData card)
        {
            if (!_accountService.TrySpendCoins(card.Cost))
                return false;

            await _effectApplier.ApplyEffect(card.Effect);

            if (card.IsExpiring)
            {
                _purchasedCardIDs.Add(card.CardID);
                _accountService.AddPurchasedCard(card.CardID);
            }

            return true;
        }
    }
}
