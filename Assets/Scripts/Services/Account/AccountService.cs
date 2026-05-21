using System.Collections.Generic;
using System.Linq;
using OrderRush.Models;
using UniRx;
using UnityEngine;

namespace OrderRush.Services
{
    public class AccountService : IAccountService
    {
        private readonly Account _account = new();
        private readonly ILocalStorageService _storage;
        private readonly IGameDataService _gameDataService;
        private List<RecipeData> _ownedRecipes = new();

        public IReadOnlyReactiveProperty<int> Coins => _account.Coins;
        public IReadOnlyList<int> OwnedRecipeIDs => _account.OwnedRecipeIDs;

        public AccountService(ILocalStorageService storage, IGameDataService gameDataService)
        {
            _storage = storage;
            _gameDataService = gameDataService;
        }

        public void Initialize()
        {
            Load();

            if (_account.OwnedRecipeIDs.Count == 0)
            {
                var defaultRecipe = _gameDataService.Recipes.Recipes.Find(r => r.IsDefaultRecipe);
                if (defaultRecipe != null)
                {
                    AddOwnedRecipe(defaultRecipe.RecipeID);
                }
                else
                {
                    Debug.LogWarning("No default recipe found in RecipesData!");
                }
            }

            SetOwnedRecipesCache();
        }

        public void AddCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError($"Cannot add negative coins: {amount}");
                return;
            }

            _account.Coins.Value += amount;
            Save();
        }

        public void SpendCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError($"Cannot spend negative coins: {amount}");
                return;
            }

            if (_account.Coins.Value < amount)
            {
                Debug.LogError($"Not enough coins. Have: {_account.Coins.Value}, Need: {amount}");
                return;
            }

            _account.Coins.Value -= amount;
            Save();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0 || _account.Coins.Value < amount)
                return false;

            _account.Coins.Value -= amount;
            Save();
            return true;
        }

        public void AddOwnedRecipe(int recipeID)
        {
            if (_account.OwnedRecipeIDs.Contains(recipeID))
                return;

            _account.OwnedRecipeIDs.Add(recipeID);

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

            return _ownedRecipes[Random.Range(0, _ownedRecipes.Count)];
        }

        private void Save()
        {
            _storage.SaveInt(LocalStorageKeys.AccountCoins, _account.Coins.Value);

            string recipeIDs = string.Join(",", _account.OwnedRecipeIDs);
            _storage.SaveString(LocalStorageKeys.AccountOwnedRecipes, recipeIDs);
        }

        private void Load()
        {
            _account.Coins.Value = _storage.LoadInt(LocalStorageKeys.AccountCoins, 0);

            string recipeIDs = _storage.LoadString(LocalStorageKeys.AccountOwnedRecipes, "");
            if (!string.IsNullOrEmpty(recipeIDs))
            {
                _account.OwnedRecipeIDs = recipeIDs.Split(',')
                    .Select(int.Parse)
                    .ToList();
            }
        }

        private void SetOwnedRecipesCache()
        {
            _ownedRecipes = _gameDataService.Recipes.Recipes
                .Where(r => _account.OwnedRecipeIDs.Contains(r.RecipeID) || r.IsDefaultRecipe)
                .ToList();

            if (_ownedRecipes.Count == 0 && _gameDataService.Recipes.Recipes.Count > 0)
            {
                _ownedRecipes.Add(_gameDataService.Recipes.Recipes[0]);
            }
        }
    }
}
