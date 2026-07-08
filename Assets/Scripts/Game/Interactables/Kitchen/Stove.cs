using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;
using VContainer;

public class Stove : CookingToolBase
{
    private IKitchenStatService _kitchenStatService;

    [Inject]
    public void ConstructStove(IKitchenStatService kitchenStatService)
    {
        _kitchenStatService = kitchenStatService;
    }

    private CancellationTokenSource _cookingCts;
    private float _cookingElapsedTime;


    protected override bool CanPlaceIngredient(IngredientData ingredient)
    {
        return ingredient != null && ingredient.Transitions.Exists(t => t.Type == TransitionType.Cook);
    }

    protected override async void StartCooking()
    {
        var transition = CurrentIngredientData.Transitions.Find(t => t.Type == TransitionType.Cook);
        if (transition == null)
        {
            Debug.Log("조리할 수 없는 재료입니다.");
            return;
        }

        Debug.Log($"[Stove] 조리 시작: {_kitchenStatService.GetModifiedDuration()}초");

        _cookingCts = new CancellationTokenSource();

        try
        {
            IsCooking = true;
            _cookingElapsedTime = 0f;

            ShowCookingGauge();

            float cookDuration = _kitchenStatService.GetModifiedDuration();
            while (_cookingElapsedTime < cookDuration)
            {
                _cookingElapsedTime += Time.deltaTime;
                float progress = _cookingElapsedTime / cookDuration;
                UpdateProgress(progress);
                await UniTask.Yield(PlayerLoopTiming.Update, _cookingCts.Token);
            }

            Debug.Log($"[Stove] 조리 완료: {transition.Result.IngredientName}");
            UpdateProgress(0f);
            await CompleteTransition(transition);

            _cookingElapsedTime = 0f;

            // 오버쿡
            _gaugeView?.SetWarning(true);

            float overcookDuration = _kitchenStatService.GetOvercookDuration();
            while (_cookingElapsedTime < overcookDuration)
            {
                _cookingElapsedTime += Time.deltaTime;
                float progress = _cookingElapsedTime / overcookDuration;
                UpdateProgress(progress);

                await UniTask.Yield(PlayerLoopTiming.Update, _cookingCts.Token);
            }

            CurrentIngredientObject.SetRuined();

        }
        catch (OperationCanceledException)
        {
            Debug.Log("요리가 중단되었습니다.");
        }
        finally
        {
            IsCooking = false;
            StopCooking();
        }

    }


    protected override void StopCooking()
    {
        base.StopCooking();
        _cookingCts?.Cancel();
        _cookingCts?.Dispose();
        _cookingCts = null;
        _cookingElapsedTime = 0;
    }

}
