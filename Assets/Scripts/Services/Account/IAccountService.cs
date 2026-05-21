using System.Collections.Generic;
using OrderRush.Data;
using UniRx;

namespace OrderRush.Services
{
    public interface IAccountService
    {
        IReadOnlyReactiveProperty<int> Coins { get; }
        IReadOnlyList<int> OwnedRecipeIDs { get; }

        void Initialize();
        void AddCoins(int amount);
        void SpendCoins(int amount);
        bool TrySpendCoins(int amount);
        void AddOwnedRecipe(int recipeID);
        RecipeData GetRandomOwnedRecipe();
    }
}
