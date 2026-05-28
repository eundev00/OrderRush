using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using OrderRush.Models;
using UniRx;
using UnityEngine;

namespace OrderRush.Services
{
    public class AccountService : IAccountService, IDisposable
    {
        private readonly ILocalStorageService _storage;
        private readonly IGameDataService _gameDataService;
        private readonly ISubscriber<PaymentEvent> _paymentSubscriber;
        private readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
        private List<RecipeData> _ownedRecipes = new();
        private readonly CompositeDisposable _disposables = new();

        public Account Account { get; private set; } = new();

        public AccountService(
            ILocalStorageService storage,
            IGameDataService gameDataService,
            ISubscriber<PaymentEvent> paymentSubscriber,
            ISubscriber<DayEndedEvent> dayEndedSubscriber)
        {
            _storage = storage;
            _gameDataService = gameDataService;
            _paymentSubscriber = paymentSubscriber;
            _dayEndedSubscriber = dayEndedSubscriber;
        }

        public void Initialize()
        {
            Load();

            if (Account.OwnedRecipeIDs.Count == 0)
            {
                var defaultRecipe = _gameDataService.Recipes.Recipes.Find(r => r.IsDefaultRecipe);
                if (defaultRecipe != null)
                {
                    Account.OwnedRecipeIDs.Add(defaultRecipe.RecipeID);
                    Save();
                }
                else
                {
                    Debug.LogWarning("No default recipe found in RecipesData!");
                }
            }

            SetOwnedRecipesCache();

            _paymentSubscriber
                .Subscribe(OnPayment)
                .AddTo(_disposables);

            _dayEndedSubscriber
                .Subscribe(OnDayEnded)
                .AddTo(_disposables);
        }

        private void OnPayment(PaymentEvent evt)
        {
            AddCoins(evt.Amount);
        }

        private void OnDayEnded(DayEndedEvent evt)
        {
            SetCurrentProgress(evt.NextDay);
        }

        public void AddCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError($"Cannot add negative coins: {amount}");
                return;
            }

            Account.Coins.Value += amount;
            Save();
        }

        public void SpendCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError($"Cannot spend negative coins: {amount}");
                return;
            }

            if (Account.Coins.Value < amount)
            {
                Debug.LogError($"Not enough coins. Have: {Account.Coins.Value}, Need: {amount}");
                return;
            }

            Account.Coins.Value -= amount;
            Save();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0 || Account.Coins.Value < amount)
                return false;

            Account.Coins.Value -= amount;
            Save();
            return true;
        }

        public void AddOwnedRecipe(int recipeID)
        {
            if (Account.OwnedRecipeIDs.Contains(recipeID))
                return;

            Account.OwnedRecipeIDs.Add(recipeID);

            var recipe = _gameDataService.Recipes.Recipes.Find(r => r.RecipeID == recipeID);
            if (recipe != null && !_ownedRecipes.Contains(recipe))
            {
                _ownedRecipes.Add(recipe);
            }

            Save();
        }

        public RecipeData GetRandomOwnedRecipe()
        {
            if (_ownedRecipes.Count == 0)
                return null;

            return _ownedRecipes[UnityEngine.Random.Range(0, _ownedRecipes.Count)];
        }

        public void SetCurrentProgress(int day)
        {
            Account.CurrentDay = day;
            Save();
        }

        public void AddPurchasedCard(int cardID)
        {
            if (!Account.PurchasedCardIDs.Contains(cardID))
            {
                Account.PurchasedCardIDs.Add(cardID);
                Save();
            }
        }

        public IReadOnlyList<int> GetPurchasedCardIDs()
        {
            return Account.PurchasedCardIDs.AsReadOnly();
        }

        private void Save()
        {
            _storage.SaveInt(LocalStorageKeys.AccountCoins, Account.Coins.Value);

            string recipeIDs = string.Join(",", Account.OwnedRecipeIDs);
            _storage.SaveString(LocalStorageKeys.AccountOwnedRecipes, recipeIDs);

            string cardIDs = string.Join(",", Account.PurchasedCardIDs);
            _storage.SaveString(LocalStorageKeys.PurchasedCardIDs, cardIDs);

            _storage.SaveInt(LocalStorageKeys.CurrentRun, Account.CurrentRun);
            _storage.SaveInt(LocalStorageKeys.CurrentDay, Account.CurrentDay);
        }

        private void Load()
        {
            Account.Coins.Value = _storage.LoadInt(LocalStorageKeys.AccountCoins, 0);

            string recipeIDs = _storage.LoadString(LocalStorageKeys.AccountOwnedRecipes, "");
            if (!string.IsNullOrEmpty(recipeIDs))
            {
                Account.OwnedRecipeIDs = recipeIDs.Split(',')
                    .Select(int.Parse)
                    .ToList();
            }

            string cardIDs = _storage.LoadString(LocalStorageKeys.PurchasedCardIDs, "");
            if (!string.IsNullOrEmpty(cardIDs))
            {
                Account.PurchasedCardIDs = cardIDs.Split(',')
                    .Select(int.Parse)
                    .ToList();
            }

            Account.CurrentRun = _storage.LoadInt(LocalStorageKeys.CurrentRun, 1);
            Account.CurrentDay = _storage.LoadInt(LocalStorageKeys.CurrentDay, 1);
        }

        private void SetOwnedRecipesCache()
        {
            _ownedRecipes = _gameDataService.Recipes.Recipes
                .Where(r => Account.OwnedRecipeIDs.Contains(r.RecipeID) || r.IsDefaultRecipe)
                .ToList();

            if (_ownedRecipes.Count == 0 && _gameDataService.Recipes.Recipes.Count > 0)
            {
                _ownedRecipes.Add(_gameDataService.Recipes.Recipes[0]);
            }
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
