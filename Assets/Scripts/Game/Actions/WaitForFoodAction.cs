using System.Threading;
using Cysharp.Threading.Tasks;

public class WaitForFoodAction : IGameAction
{
    private readonly CustomerCharacter _character;

    public WaitForFoodAction(CustomerCharacter character, WorldUIFactory worldUIFactory, IGameDataService gameDataService)
    {
        _character = character;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
            return;

        if (_character.OrderedRecipeID == -1)
            return;

        await UniTask.WaitUntilCanceled(ct);
    }
}
