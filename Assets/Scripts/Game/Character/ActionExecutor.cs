using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ActionExecutor : MonoBehaviour
{
    private readonly Queue<IGameAction> _actionQueue = new();
    private CancellationTokenSource _executionCts;
    private bool _isExecuting;

    void OnEnable()
    {
        StartExecutionLoop().Forget();
    }

    void OnDisable()
    {
        Clear();
    }

    public void Enqueue(IGameAction action)
    {
        if (action == null)
        {
            Debug.LogWarning("Cannot enqueue null action");
            return;
        }

        _actionQueue.Enqueue(action);
    }

    public void Clear()
    {
        _actionQueue.Clear();

        // 현재 실행 중인 액션 취소
        _executionCts?.Cancel();
        _executionCts?.Dispose();
        _executionCts = null;
    }

    private async UniTask StartExecutionLoop()
    {
        while (Application.isPlaying)
        {
            if (_actionQueue.Count > 0)
            {
                _isExecuting = true;
                var action = _actionQueue.Dequeue();

                _executionCts = new CancellationTokenSource();

                try
                {
                    await action.ExecuteAsync(_executionCts.Token);
                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log("Action cancelled");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Action execution failed: {e}");
                }
                finally
                {
                    _executionCts?.Dispose();
                    _executionCts = null;
                    _isExecuting = false;
                }
            }

            await UniTask.Yield();
        }
    }

    public int GetQueueCount() => _actionQueue.Count;
    public bool IsExecuting() => _isExecuting;
}
