using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ActionExecutor : MonoBehaviour
{
    private readonly Queue<IGameAction> _actionQueue = new();
    public IGameAction CurrentAction { get; private set; }
    private CancellationTokenSource _executionCts;
    private CancellationTokenSource _loopCts;
    private bool _isExecuting;

    public event Action ExecutionCompleted;

    public void StartLoop()
    {
        if (_loopCts != null) return;

        _loopCts = new CancellationTokenSource();
        ExecutionLoop(_loopCts.Token).Forget();
    }

    public void StopLoop()
    {
        Clear();

        _loopCts?.Cancel();
        _loopCts?.Dispose();
        _loopCts = null;
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
        CancelCurrentAction();
        _actionQueue.Clear();
    }

    private async UniTask ExecutionLoop(CancellationToken loopCt)
    {
        while (!loopCt.IsCancellationRequested)
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

                    if (_actionQueue.Count == 0)
                        ExecutionCompleted?.Invoke();
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update, loopCt);
        }
    }

    public int GetQueueCount() => _actionQueue.Count;
    public bool IsExecuting() => _isExecuting;
}
