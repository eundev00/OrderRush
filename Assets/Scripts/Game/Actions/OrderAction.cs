using System.Threading;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;

public class OrderAction : IGameAction
{
    private readonly CustomerCharacter _character;
    private readonly IAccountService _accountService;
    private readonly IGameDataService _gameDataService;

    public OrderAction(CustomerCharacter character, IAccountService accountService, IGameDataService gameDataService)
    {
        _character = character;
        _accountService = accountService;
        _gameDataService = gameDataService;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
        {
            return;
        }

        var recipe = _accountService.GetRandomOwnedRecipe();

        if (recipe == null)
        {
            Debug.LogWarning("No owned recipes available!");
            return;
        }

        if (_character != null && _character.gameObject != null)
        {
            _character.Order = new Order(recipe, _gameDataService.Config.OrderWaitDuration);
        }

        await UniTask.CompletedTask;
    }
}
