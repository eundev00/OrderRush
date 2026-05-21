using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;
using VContainer;

public class DishSink : InteractableBase
{
    [NotNull][SerializeField] Transform _plateSlot;
    [NotNull][SerializeField] Canvas _canvas;
    [NotNull][SerializeField] CookingProgressView _progressView;

    private Plate _currentPlate;
    private IGameDataService _gameDataService;

    [Inject]
    public void Construct(IGameDataService gameDataService)
    {
        _gameDataService = gameDataService;
    }

    void Awake()
    {
        _canvas.worldCamera = Camera.main;
    }

    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;



        await UniTask.CompletedTask;
    }

    private async UniTask StartWashing(CancellationToken ct)
    {
        _progressView.SetCookingStyle();
        _progressView.SetVisible(true);

        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < _gameDataService.Config.ToolProcessSeconds)
            {
                // 취소 요청 확인
                if (ct.IsCancellationRequested)
                {
                    Debug.Log("[DishSink] Washing cancelled");
                    StopWashing();
                    throw new OperationCanceledException();
                }

                await UniTask.Yield();
                elapsedTime += Time.deltaTime;

                float progress = elapsedTime / _gameDataService.Config.ToolProcessSeconds;
                _progressView.SetProgress(progress);
            }

            // 설거지 완료
            _currentPlate.SetClean();
            _progressView.SetVisible(false);
            Debug.Log("[DishSink] Washing completed");
        }
        catch (OperationCanceledException)
        {
            // 취소된 경우 예외를 다시 throw
            throw;
        }
    }

    private void StopWashing()
    {
        _progressView.SetVisible(false);
        _progressView.SetProgress(0);
    }

}
