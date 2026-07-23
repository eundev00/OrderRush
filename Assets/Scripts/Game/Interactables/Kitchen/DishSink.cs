using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class DishSink : InteractableBase
{
    [NotNull][SerializeField] Transform _plateSlot;
    [NotNull][SerializeField] CookingGauge _cookingGauge;

    private Plate _currentPlate;
    private IKitchenStatService _kitchenStatService;

    [Inject]
    public void Construct(IKitchenStatService kitchenStatService)
    {
        _kitchenStatService = kitchenStatService;
    }

    void Awake()
    {
        _cookingGauge.Hide();
    }


    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;

        // 1. 더러운 접시를 들고 있는 경우 → 싱크대에 내려놓고 설거지 시작
        if (character.IsHolding && character.CurrentCarriable is Plate plate && plate.IsDirty)
        {
            await character.PutDownAt(_plateSlot);
            _currentPlate = plate;

            try
            {
                await StartWashing(character, ct);

                // 완료 후 자동으로 집기
                await character.PickUp(_currentPlate);
                _currentPlate = null;
            }
            catch (OperationCanceledException)
            {
                // 취소되면 접시는 싱크대에 그대로 남음
                Debug.Log("[DishSink] Washing cancelled, plate remains in sink");
            }
            return;
        }

        // 2. 빈 손 + 싱크대에 더러운 접시가 있음 → 설거지 재시작
        if (!character.IsHolding && _currentPlate != null && _currentPlate.IsDirty)
        {
            try
            {
                await StartWashing(character, ct);

                // 완료 후 자동으로 집기
                await character.PickUp(_currentPlate);
                _currentPlate = null;
            }
            catch (OperationCanceledException)
            {
                // 취소되면 접시는 싱크대에 그대로 남음
                Debug.Log("[DishSink] Washing cancelled, plate remains in sink");
            }
            return;
        }

        // 3. 빈 손 + 싱크대에 깨끗한 접시가 있음 → 집기
        if (!character.IsHolding && _currentPlate != null && !_currentPlate.IsDirty)
        {
            await character.PickUp(_currentPlate);
            _currentPlate = null;
            return;
        }


        await UniTask.CompletedTask;
    }

    private async UniTask StartWashing(CharacterBase character, CancellationToken ct)
    {
        _cookingGauge.Show();

        float elapsedTime = 0f;
        float washDuration = _kitchenStatService.GetModifiedDuration();

        try
        {
            character.StartWorking();

            while (elapsedTime < washDuration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                elapsedTime += Time.deltaTime;

                float progress = elapsedTime / washDuration;
                _cookingGauge.SetProgress(progress);
            }

            // 설거지 완료
            _currentPlate.SetClean();
            Debug.Log("[DishSink] Washing completed");
        }
        finally
        {
            character.StopWorking();
            _cookingGauge.Hide();
        }
    }

}
