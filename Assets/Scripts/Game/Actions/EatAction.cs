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

    public EatAction(CustomerCharacter customer, IPublisher<PaymentEvent> paymentPublisher, IGameDataService gameDataService)
    {
        _customer = customer;
        _paymentPublisher = paymentPublisher;
        _gameDataService = gameDataService;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_customer == null || _customer.gameObject == null)
        {
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_gameDataService.Config.EatDuration), cancellationToken: ct);

        if (_customer != null && _customer.gameObject != null && _customer.Order?.Recipe != null)
        {
            int amount = _customer.Order.Recipe.SellPrice;
            _paymentPublisher.Publish(new PaymentEvent(amount, _customer.Order.Recipe.RecipeName));
        }
    }
}
