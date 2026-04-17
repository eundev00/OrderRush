using System.Threading;
using Cysharp.Threading.Tasks;

public class SitAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly DiningSeat _seat;

    public SitAction(CustomerCharacter customer, DiningSeat seat)
    {
        _customer = customer;
        _seat = seat;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_seat.SitPoint == null)
        {
            UnityEngine.Debug.LogError("[SitAction] SitPoint is null!");
            return;
        }

        // 앉을 때는 NavMeshAgent 끄기 (NavMesh 위가 아닌 위치에 배치하기 위해)
        _customer.DisableNavMeshAgent();

        // 직접 위치/회전 설정
        _customer.transform.position = _seat.SitPoint.position;
        _customer.transform.rotation = _seat.SitPoint.rotation;
        _seat.SeatCustomer(_customer);

        await UniTask.CompletedTask;
    }
}
