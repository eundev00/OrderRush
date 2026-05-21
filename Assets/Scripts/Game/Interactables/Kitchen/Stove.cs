using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Stove : CookingToolBase
{

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

        Debug.Log($"[Stove] 조리 시작: {_gameDataService.Config.ToolProcessSeconds}초");

        _cookingCts = new CancellationTokenSource();

        try
        {
            IsCooking = true;
            _cookingElapsedTime = 0f;

            ShowCookingGauge();

            while (_cookingElapsedTime < _gameDataService.Config.ToolProcessSeconds)
            {
                _cookingElapsedTime += Time.deltaTime;
                float progress = _cookingElapsedTime / _gameDataService.Config.ToolProcessSeconds;
                UpdateProgress(progress);
                await UniTask.Yield(PlayerLoopTiming.Update, _cookingCts.Token);
            }

            // 조리 완료
            Debug.Log($"[Stove] 조리 완료: {transition.Result.IngredientName}");
            UpdateProgress(0f);
            await CompleteTransition(transition);

            _cookingElapsedTime = 0f;

            // 주황색으로 색상 변경 (오버쿡)
            _gaugePresenter?.SetColor(new Color(1f, 0.5f, 0f));

            // 오버쿡 타이머
            while (_cookingElapsedTime < _gameDataService.Config.ToolProcessSeconds)
            {
                _cookingElapsedTime += Time.deltaTime;
                float progress = _cookingElapsedTime / _gameDataService.Config.ToolProcessSeconds;
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