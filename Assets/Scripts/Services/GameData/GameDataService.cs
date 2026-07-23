using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OrderRush.Data;

public class GameDataService : IGameDataService
{
    private readonly IResourcesLoaderService _resourcesLoader;

    public GameConfig Config { get; private set; }
    public RecipesData Recipes { get; private set; }
    public CardsData Cards { get; private set; }
    public DaysData Days { get; private set; }
    public DayNightSettings DayNightSettings { get; private set; }

    public GameDataService(IResourcesLoaderService resourcesLoader)
    {
        _resourcesLoader = resourcesLoader;
    }

    public async UniTask Initialize()
    {
        Config = await _resourcesLoader.LoadAsync<GameConfig>(DataKeys.GetDataPath(DataKeys.GameConfig));
        Recipes = await _resourcesLoader.LoadAsync<RecipesData>(DataKeys.GetDataPath(DataKeys.RecipesData));
        Cards = await _resourcesLoader.LoadAsync<CardsData>(DataKeys.GetDataPath(DataKeys.CardsData));
        Days = await _resourcesLoader.LoadAsync<DaysData>(DataKeys.GetDataPath(DataKeys.Run1_Days));
        DayNightSettings = await _resourcesLoader.LoadAsync<DayNightSettings>(DataKeys.GetDataPath(DataKeys.DayNightSettings));
    }

    public RecipeData GetRecipeByID(int recipeID)
    {
        return Recipes.Recipes.Find(r => r.RecipeID == recipeID);
    }

    public int GetMatchedRecipeID(List<IngredientData> ingredients)
    {
        foreach (var recipe in Recipes.Recipes)
        {
            if (recipe.IsMatch(ingredients))
                return recipe.RecipeID;
        }
        return -1;
    }

    public List<int> GetDefaultRecipeIDs()
    {
        List<int> defaultRecipeIDs = new List<int>();
        foreach (var recipe in Recipes.Recipes)
        {
            if (recipe.IsDefaultRecipe)
                defaultRecipeIDs.Add(recipe.RecipeID);
        }

        if (defaultRecipeIDs.Count == 0)
            defaultRecipeIDs.Add(Recipes.Recipes[0].RecipeID);
        return defaultRecipeIDs;
    }

    public CardData GetCardByID(int cardID) => Cards.Cards.Find(c => c.CardID == cardID);

    public List<CardData> GetAllCards() => Cards.Cards;

    public int GetRefreshCost(int refreshCount) => Config.GetRefreshCost(refreshCount);
}
