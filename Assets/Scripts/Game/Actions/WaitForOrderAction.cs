using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaitForOrderAction : IGameAction
{
    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        try
        {
            await UniTask.WaitUntilCanceled(ct);
        }
        finally
        {
        }
    }
}
