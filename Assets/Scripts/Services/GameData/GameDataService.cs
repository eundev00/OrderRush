using Cysharp.Threading.Tasks;
using OrderRush.Data;

namespace OrderRush.Services
{
    public class GameDataService : IGameDataService
    {
        private readonly IResourcesLoaderService _resourcesLoader;
        private GameConfig _config;
        private RecipesData _recipesData;
        private DaysData _daysData;

        public GameConfig Config => _config;
        public RecipesData Recipes => _recipesData;
        public DaysData Days => _daysData;

        public GameDataService(IResourcesLoaderService resourcesLoader)
        {
            _resourcesLoader = resourcesLoader;
        }

        public async UniTask Initialize()
        {
            _config = await _resourcesLoader.LoadAsync<GameConfig>(DataKeys.GetDataPath(DataKeys.GameConfig));
            _recipesData = await _resourcesLoader.LoadAsync<RecipesData>(DataKeys.GetDataPath(DataKeys.Recipes));
            _daysData = await _resourcesLoader.LoadAsync<DaysData>(DataKeys.GetDataPath(DataKeys.Run1_Days));
        }
    }
}
