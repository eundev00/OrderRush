using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OrderRush.Data;

namespace OrderRush.Services
{
    public interface IGameDataService
    {
        GameConfig Config { get; }
        DaysData Days { get; }
        DayNightSettings DayNightSettings { get; }

        RecipeData GetRecipeByID(int recipeID);
        int GetMatchedRecipeID(List<IngredientData> ingredients);
        List<int> GetDefaultRecipeIDs();

        CardData GetCardByID(int cardID);
        List<CardData> GetAllCards();
        int GetRefreshCost(int refreshCount);

        UniTask Initialize();
    }
}
