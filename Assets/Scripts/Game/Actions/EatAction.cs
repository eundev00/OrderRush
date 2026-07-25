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
        if (_customer == null || _customer.gameObject == null)
            return;

        await UniTask.Delay(TimeSpan.FromSeconds(_gameDataService.Config.EatDuration), cancellationToken: ct);

        if (_customer != null && _customer.gameObject != null && _customer.OrderedRecipeID != -1)
        {
            var recipe = _gameDataService.GetRecipeByID(_customer.OrderedRecipeID);
            if (recipe != null)
            {
                // TODO: 실제로는 CustomerCharacterData.GivesTip 확인 필요
                bool hasTip = false;

                var coinObj1 = _worldUIFactory.Create(PrefabKeys.FloatingCoinFX);
                var coinFX1 = coinObj1.GetComponent<FloatingCoinFX>();
                var task1 = coinFX1.PlayAnimation(ct);

                _paymentPublisher.Publish(new PaymentEvent(recipe.SellPrice, recipe.RecipeName));

                if (hasTip)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ct);

                    var coinObj2 = _worldUIFactory.Create(PrefabKeys.FloatingCoinFX);
                    var coinFX2 = coinObj2.GetComponent<FloatingCoinFX>();
                    var task2 = coinFX2.PlayAnimation(ct);

                    await UniTask.WhenAll(task1, task2);

                    _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinObj1);
                    _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinObj2);
                }
                else
                {
                    await task1;
                    _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinObj1);
                }
            }
        }
    }
}
