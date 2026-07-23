using System.Threading;
using Cysharp.Threading.Tasks;

public class WaitForFoodAction : IGameAction
{
    private readonly CustomerCharacter _character;
    private readonly IGameDataService _gameDataService;

    public WaitForFoodAction(CustomerCharacter character, IGameDataService gameDataService)
    {
        _character = character;
        _gameDataService = gameDataService;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
            return;

        if (_character.OrderedRecipeID == -1)
            return;

        try
        {
            var recipe = _gameDataService.GetRecipeByID(_character.OrderedRecipeID);
            if (recipe != null)
            {
                _character.OrderBubble.Show(recipe);
            }

            await UniTask.WaitUntilCanceled(ct);
        }
        finally
        {
            _character.OrderBubble.Hide();
        }
    }
}
