using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Stove : CookingToolBase
{
    public override string DisplayName => "Stove";


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
            Debug.Log($"[{DisplayName}] 조리할 수 없는 재료입니다.");
            return;
        }

        Debug.Log($"[Stove] 조리 시작: {transition.Duration}초");

        _cookingCts = new CancellationTokenSource();

        try
        {
            IsCooking = true;
            _progressView.SetCookingStyle();

            while (_cookingElapsedTime < transition.Duration)
            {
                _cookingElapsedTime += Time.deltaTime;
                UpdateProgress(_cookingElapsedTime, transition.Duration);
                await UniTask.Yield(PlayerLoopTiming.Update, _cookingCts.Token);
            }

            // 조리 완료
            Debug.Log($"[Stove] 조리 완료: {transition.Result.IngredientName}");
            _progressView.SetProgress(1f);
            await CompleteTransition(transition);

            _cookingElapsedTime = 0f;
            _progressView.SetOverdoneStyle();

            // 오버쿡 타이머
            while (_cookingElapsedTime < transition.OverDuration)
            {
                _cookingElapsedTime += Time.deltaTime;
                UpdateProgress(_cookingElapsedTime, transition.OverDuration);

                await UniTask.Yield(PlayerLoopTiming.Update, _cookingCts.Token);
            }

            _currentIngredientObject.SetRuined();
            _progressView.SetVisible(false);

        }
        catch (OperationCanceledException)
        {
            Debug.Log($"[{DisplayName}] 요리가 중단되었습니다.");
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
        _progressView.SetVisible(false);
    }


}