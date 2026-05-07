using System.Threading;
using Cysharp.Threading.Tasks;

public class SitAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly DiningTable _table;
    private readonly int _seatIndex;

    public SitAction(CustomerCharacter customer, DiningTable seat, int seatIndex)
    {
        _customer = customer;
        _table = seat;
        _seatIndex = seatIndex;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_table == null)
        {
            UnityEngine.Debug.LogError("[SitAction] DiningTable is null!");
            return;
        }

        _customer.DisableNavMeshAgent();

        var seatTransform = _table.GetSeatTransform(_seatIndex);
        if (seatTransform != null)
        {
            _customer.transform.position = seatTransform.position;
            _customer.transform.rotation = seatTransform.rotation;
            _table.SeatCustomer(_customer, _seatIndex);
        }
        else
        {
            UnityEngine.Debug.LogError($"[SitAction] Invalid seat index: {_seatIndex}");
            return;
        }


        await UniTask.CompletedTask;
    }
}
