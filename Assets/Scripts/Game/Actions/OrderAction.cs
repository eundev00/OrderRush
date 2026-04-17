using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OrderAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly RecipeData _recipe;

    public OrderAction(CustomerCharacter customer, RecipeData recipe)
    {
        _customer = customer;
        _recipe = recipe;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        // Order 생성 (착석 완료 후 실행됨)
        _customer.CreateOrder(_recipe);

        // TODO: 여기서 머리 위 UI 표시
        // ShowOrderUI();

        await UniTask.CompletedTask;
    }
}
