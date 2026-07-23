using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OrderAction : IGameAction
{
    private readonly CustomerCharacter _character;
    private readonly IAccountService _accountService;

    public OrderAction(CustomerCharacter character, IAccountService accountService)
    {
        _character = character;
        _accountService = accountService;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
            return;

        int recipeID = _accountService.GetRandomOwnedRecipeID();

        if (recipeID == -1)
        {
            Debug.LogWarning("No owned recipes available!");
            return;
        }

        if (_character != null && _character.gameObject != null)
        {
            _character.OrderedRecipeID = recipeID;
            _character.OrderedTime = Time.time;
        }

        await UniTask.CompletedTask;
    }
}
