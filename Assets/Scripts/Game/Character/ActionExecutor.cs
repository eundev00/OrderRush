using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ActionExecutor : MonoBehaviour
{
    private readonly Queue<IGameAction> _actionQueue = new();
    public IGameAction CurrentAction { get; private set; }
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

    public void CancelCurrentAction()
    {
        _executionCts?.Cancel();
        _executionCts?.Dispose();
        _executionCts = null;
    }

    public void Clear()
    {
        _actionQueue.Clear();
        CancelCurrentAction();
    }

    private async UniTask StartExecutionLoop()
    {
        while (Application.isPlaying)
        {
            if (_actionQueue.Count > 0)
            {
                _isExecuting = true;
                CurrentAction = _actionQueue.Dequeue();

                _executionCts = new CancellationTokenSource();

                try
                {
                    await CurrentAction.ExecuteAsync(_executionCts.Token);
                }
                catch (System.OperationCanceledException)
                {
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
                    CurrentAction = null;
                }
            }

            await UniTask.Yield();
        }
    }

    public int GetQueueCount() => _actionQueue.Count;
    public bool IsExecuting() => _isExecuting;
}
