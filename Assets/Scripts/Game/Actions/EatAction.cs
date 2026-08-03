using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

public class EatAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly IPublisher<PaymentEvent> _paymentPublisher;
    private readonly IGameDataService _gameDataService;
    private readonly WorldUIFactory _worldUIFactory;

    public EatAction(CustomerCharacter customer, IPublisher<PaymentEvent> paymentPublisher, IGameDataService gameDataService, WorldUIFactory worldUIFactory)
    {
        _customer = customer;
        _paymentPublisher = paymentPublisher;
        _gameDataService = gameDataService;
        _worldUIFactory = worldUIFactory;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_gameDataService.Config.EatDuration), cancellationToken: ct);

            var table = _customer.AssignedTable;
            if (table != null)
                table.RemovePlateFood(_customer.AssignedSeatIndex);

            var recipe = _gameDataService.GetRecipeByID(_customer.OrderedRecipeID);
            if (recipe == null)
                return;

            var headPosition = _customer.transform.position + Vector3.up * 2f;

            // TODO: 실제로는 CustomerCharacterData.GivesTip 확인 필요
            bool hasTip = false;

            var task1 = PlayCoinFX(headPosition, ct);
            _paymentPublisher.Publish(new PaymentEvent(recipe.SellPrice, recipe.RecipeName));

            if (hasTip)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ct);
                var task2 = PlayCoinFX(headPosition, ct);
                await UniTask.WhenAll(task1, task2);
            }
            else
            {
                await task1;
            }
        }
        catch (OperationCanceledException) { }
        catch (MissingReferenceException) { }
    }

    private async UniTask PlayCoinFX(Vector3 position, CancellationToken ct)
    {
        var coinObj = _worldUIFactory.Create(PrefabKeys.FloatingCoinFX);
        coinObj.transform.position = position;
        var coinFX = coinObj.GetComponent<FloatingCoinFX>();
        await coinFX.PlayAnimation(ct);
        _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinObj);
    }
}
