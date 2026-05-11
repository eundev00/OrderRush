using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

public class EatAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly IPublisher<PaymentEvent> _paymentPublisher;
    private readonly float _eatDuration = 3.0f;

    public EatAction(CustomerCharacter customer, IPublisher<PaymentEvent> paymentPublisher)
    {
        _customer = customer;
        _paymentPublisher = paymentPublisher;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        Debug.Log($"[EatAction] {_customer.name} starts eating for {_eatDuration}s");

        await UniTask.Delay(TimeSpan.FromSeconds(_eatDuration), cancellationToken: ct);

        Debug.Log($"[EatAction] {_customer.name} finished eating");

        if (_customer.Order?.Recipe != null)
        {
            int amount = _customer.Order.Recipe.Price;
            _paymentPublisher.Publish(new PaymentEvent(amount, _customer.Order.Recipe.RecipeName));

            Debug.Log($"[EatAction] {_customer.name} paid {amount}");
        }
    }
}
