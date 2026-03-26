using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CookingService : ICookingService
{
    private readonly Dictionary<CookingToolBase, CookingProcess> _activeCookings = new();
    private readonly Dictionary<CookingToolBase, CancellationTokenSource> _burnCts = new();
    private const float BURN_DELAY = 5f;

    public void RegisterTool(CookingToolBase tool)
    {
        tool.OnCookingCompleted += OnToolCookingCompleted;
    }

    public void UnregisterTool(CookingToolBase tool)
    {
        tool.OnCookingCompleted -= OnToolCookingCompleted;
    }

    private void OnToolCookingCompleted(CookingToolBase tool, CookingStep step)
    {
        if (tool.CurrentIngredient != null)
        {
            tool.CurrentIngredient.State = step.resultState;
            Debug.Log($"{tool.DisplayName} 조리 완료! {tool.CurrentIngredient}");
        }

        _activeCookings.Remove(tool);
        BurnDelayAsync(tool).Forget();
    }



    private async UniTask BurnDelayAsync(CookingToolBase tool)
    {
        _burnCts[tool]?.Cancel();
        _burnCts[tool]?.Dispose();
        _burnCts[tool] = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(BURN_DELAY),
                               cancellationToken: _burnCts[tool].Token);

            if (tool.CurrentIngredient != null && !tool.IsCooking)
            {
                tool.CurrentIngredient.State = IngredientState.Burnt;
                Debug.LogWarning($"{tool.DisplayName}의 {tool.CurrentIngredient}이(가) 타버렸습니다!");
            }
        }
        catch (System.OperationCanceledException) { }
    }

    public void CancelBurn(CookingToolBase tool)
    {
        if (_burnCts.TryGetValue(tool, out var cts))
        {
            cts?.Cancel();
            cts?.Dispose();
            _burnCts.Remove(tool);
        }
    }

    public void StartCooking(CookingToolBase tool, CookingStep step)
    {
        _activeCookings[tool] = new CookingProcess
        {
            Step = step,
            Cts = null,
            ElapsedTime = 0f
        };
    }

    public void StopCooking(CookingToolBase tool)
    {
        _activeCookings.Remove(tool);
    }

    public float GetCookingProgress(CookingToolBase tool)
    {
        return tool.GetProgress();
    }

    public bool IsCooking(CookingToolBase tool)
    {
        return tool.IsCooking;
    }
}
