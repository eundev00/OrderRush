using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaitForFoodAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    public WaitForFoodAction(CustomerCharacter customer)
    {
        _customer = customer;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        float elapsed = 0f;

        while (elapsed < CustomerCharacter.WAIT_TIME_LIMIT)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / CustomerCharacter.WAIT_TIME_LIMIT;
            _customer.SetWaitGauge(progress);
            await UniTask.Yield();
        }

        // 시간 초과 → 이탈 처리
        _customer.OnWaitTimeout();

        await UniTask.CompletedTask;
    }
}
