using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DiningTable : MonoBehaviour, IInteractable
{
    [SerializeField] Transform _interactPoint;
    [SerializeField] Transform[] _plateSlots;
    [SerializeField] DiningSeat[] _seats;

    public string DisplayName => "DiningTable";
    public Transform InteractPoint => _interactPoint;
    private List<Plate> _currentPlates = new List<Plate>();

    void Awake()
    {
        for (int i = 0; i < _seats.Length; i++)
        {
            _seats[i].Init(this, i);
        }
    }

    public void PlacePlate(int seatIndex, Plate plate)
    {
        _currentPlates[seatIndex] = plate;
        plate.transform.position = _plateSlots[seatIndex].position;
    }

    public DiningSeat GetAvailableSeat()
    {
        return _seats.FirstOrDefault(seat => !seat.HasCustomer);
    }


    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        // 플레이어가 음식을 들고 있으면 → 서빙
        if (character.IsHolding)
        {
            await ServeFood(character);
        }
        // 플레이어가 빈손이면 → 접시 픽업
        else
        {
            await PickUpPlate(character);
        }
    }

    private async UniTask ServeFood(CharacterBase character)
    {
        var plate = character.CurrentCarriable as Plate;
        if (plate == null) return;

        var ingredientDatas = plate.PlacedIngredients.Select(obj => obj.Data).ToList();

        // 모든 좌석을 순회하며 주문과 맞는 손님 찾기
        foreach (var seat in _seats)
        {
            if (!seat.HasCustomer) continue;

            var customer = seat.CurrentCustomer;
            if (customer.Order.Recipe.IsComplete(ingredientDatas))
            {
                // 주문과 일치! 서빙 처리
                character.PutDown();
                PlacePlate(seat.GetSeatIndex(), plate);

                // Order 완료 처리
                customer.Order.Complete();
                Debug.Log($"[DiningTable] Order completed: {customer.Order.Recipe.RecipeName}");

                seat.Clear();

                Debug.Log($"[DiningTable] Served food to customer at seat {seat.GetSeatIndex()}");
                await UniTask.CompletedTask;
                return;
            }
        }

        // 일치하는 주문 없음
        Debug.LogWarning("[DiningTable] No matching order found!");
        await UniTask.CompletedTask;
    }

    private async UniTask PickUpPlate(CharacterBase character)
    {
        Plate plate = null;
        int plateIndex = -1;

        for (int i = 0; i < _currentPlates.Count; i++)
        {
            if (_currentPlates[i] != null)
            {
                plate = _currentPlates[i];
                plateIndex = i;
                break;
            }
        }

        if (plate == null) return;

        character.PickUp(plate);
        _currentPlates[plateIndex] = null;

        await UniTask.CompletedTask;
    }
}
