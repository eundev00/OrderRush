using OrderRush.Data;
using OrderRush.Models;

namespace OrderRush.Services
{
    public interface IAccountService
    {
        Account Account { get; }

        void Initialize();
        void AddCoins(int amount);
        void SpendCoins(int amount);
        bool TrySpendCoins(int amount);
        void AddOwnedRecipe(int recipeID);
        RecipeData GetRandomOwnedRecipe();
        void SetCurrentProgress(int day);
    }
}
