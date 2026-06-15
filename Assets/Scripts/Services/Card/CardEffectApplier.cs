using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OrderRush.Services
{
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
            var allCards = _gameDataService.Cards.Cards;

            foreach (var id in purchasedIDs)
            {
                var card = allCards.Find(c => c.CardID == id);
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
                    await ApplyTableAddition((TableAdditionEffect)effect);
                    break;
                case EffectType.Menu:
                    ApplyMenuUnlock((MenuCardEffect)effect);
                    break;
                case EffectType.ToolUpgrade:
                    ApplyUpgrade((UpgradeCardEffect)effect);
                    break;
                case EffectType.StaffHire:
                    break;
                case EffectType.OvercookExtend:
                    ApplyOvercookExtend((OvercookCardEffect)effect);
                    break;
            }
        }

        private async UniTask ApplyTableAddition(TableAdditionEffect effect)
        {
            Vector3 position = CalculateNewTablePosition();

            var table = await _spawnFactory.CreatePersistent<DiningTable>(
                effect.TablePrefabName,
                position,
                _levelPresenter.LevelTransform);

            if (table != null)
            {
                _levelPresenter.AddDiningTable(table);
            }
        }

        private void ApplyMenuUnlock(MenuCardEffect effect)
        {
            _accountService.AddOwnedRecipe(effect.Recipe.RecipeID);
        }

        private void ApplyUpgrade(UpgradeCardEffect effect)
        {
            _kitchenStatService.AddDurationReduce(effect.DurationReducePercent);
        }

        private void ApplyOvercookExtend(OvercookCardEffect effect)
        {
            _kitchenStatService.AddOvercookExtend(effect.ExtendPercent);
        }

        private Vector3 CalculateNewTablePosition()
        {
            var tables = _levelPresenter.DiningTables;
            if (tables.Count == 0)
                return Vector3.zero;

            var lastTable = tables[tables.Count - 1];
            return lastTable.transform.position + new Vector3(2f, 0, 0);
        }
    }
}
