using Cysharp.Threading.Tasks;
using OrderRush.Data;

namespace OrderRush.Services
{
    public interface IGameDataService
    {
        GameConfig Config { get; }
        RecipesData Recipes { get; }
        DaysData Days { get; }

        UniTask Initialize();
    }
}
