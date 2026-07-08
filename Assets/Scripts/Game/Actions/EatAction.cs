using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Services;
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
        if (_customer == null || _customer.gameObject == null)
            return;

        await UniTask.Delay(TimeSpan.FromSeconds(_gameDataService.Config.EatDuration), cancellationToken: ct);

        if (_customer != null && _customer.gameObject != null && _customer.OrderedRecipeID != -1)
        {
            var recipe = _gameDataService.GetRecipeByID(_customer.OrderedRecipeID);
            if (recipe != null)
            {
                var coinFX = _worldUIFactory.Create<FloatingCoinFX>(PrefabKeys.FloatingCoinFX);

                Vector3 coinPosition = _customer.transform.position + new Vector3(0, 2f, 0);
                await coinFX.PlayAnimation(coinPosition, ct);

                _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinFX);
                _paymentPublisher.Publish(new PaymentEvent(recipe.SellPrice, recipe.RecipeName));
            }
        }
    }
}
