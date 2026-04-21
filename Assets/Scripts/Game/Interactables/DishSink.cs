using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DishSink : MonoBehaviour, IInteractable
{
    [NotNull][SerializeField] Transform _interactPoint;
    [NotNull][SerializeField] Transform _plateSlot;
    [NotNull][SerializeField] Canvas _canvas;
    [NotNull][SerializeField] CookingProgressView _progressView;

    private Plate _currentPlate;
    private const float WASH_DURATION = 3f;

    public string DisplayName => "Dish Sink";
    public Transform InteractPoint => _interactPoint;

    void Awake()
    {
        _canvas.worldCamera = Camera.main;
    }

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;

        // 더러운 접시를 들고 있으면 → 설거지 시작
        if (character.IsHolding && character.CurrentCarriable is Plate plate && plate.IsDirty)
        {
            character.PutDown();
            _currentPlate = plate;
            plate.transform.SetParent(_plateSlot);
            plate.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            Debug.Log("[DishSink] Starting to wash plate");
            await StartWashing(ct);
        }
        // 빈손이고 깨끗한 접시가 있으면 → 접시 들기
        else if (!character.IsHolding && _currentPlate != null && !_currentPlate.IsDirty)
        {
            character.PickUp(_currentPlate);
            _currentPlate = null;
            Debug.Log("[DishSink] Clean plate picked up");
        }
        else
        {
            Debug.Log($"[DishSink] Cannot interact - IsHolding: {character.IsHolding}, CurrentPlate: {_currentPlate}, IsDirty: {_currentPlate?.IsDirty}");
        }

        await UniTask.CompletedTask;
    }

    private async UniTask StartWashing(CancellationToken ct)
    {
        _progressView.SetCookingStyle();
        _progressView.SetVisible(true);

        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < WASH_DURATION)
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

                float progress = elapsedTime / WASH_DURATION;
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
