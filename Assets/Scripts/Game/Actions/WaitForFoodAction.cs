using System.Threading;
using Cysharp.Threading.Tasks;
using OrderRush.Services;

public class WaitForFoodAction : IGameAction
{
    private readonly CustomerCharacter _character;
    private readonly WorldUIFactory _worldUIFactory;
    private readonly IGameDataService _gameDataService;
    private OrderIcon _orderIcon;

    public WaitForFoodAction(CustomerCharacter character, WorldUIFactory worldUIFactory, IGameDataService gameDataService)
    {
        _character = character;
        _worldUIFactory = worldUIFactory;
        _gameDataService = gameDataService;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
            return;

        if (_character.OrderedRecipeID == -1)
            return;

        var recipe = _gameDataService.GetRecipeByID(_character.OrderedRecipeID);
        if (recipe == null)
            return;

        try
        {
            _orderIcon = _worldUIFactory.Create<OrderIcon>(
                PrefabKeys.CharacterOrderIcon,
                _character.transform,
                new UnityEngine.Vector3(0, 1.5f, 0));
            _orderIcon.SetIcon(recipe.Icon);

            await UniTask.WaitUntilCanceled(ct);
        }
        finally
        {
            if (_orderIcon != null)
            {
                _worldUIFactory.Release(PrefabKeys.CharacterOrderIcon, _orderIcon);
                _orderIcon = null;
            }
        }
    }
}
