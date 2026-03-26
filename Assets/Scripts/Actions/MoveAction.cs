using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveAction : IGameAction
{
    private readonly NavMeshMover _mover;
    private readonly Vector3 _destination;

    public MoveAction(NavMeshMover mover, Vector3 destination)
    {
        _mover = mover;
        _destination = destination;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        await _mover.MoveToAsync(_destination, ct);
    }
}
