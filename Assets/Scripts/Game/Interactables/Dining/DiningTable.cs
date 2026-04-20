using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using JetBrains.Annotations;

public class DiningTable : MonoBehaviour, IInteractable
{
    [NotNull][SerializeField] Transform _interactPoint;
    [NotNull][SerializeField] Transform[] _plateSlots;
    [NotNull][SerializeField] DiningSeat[] _seats;

    public string DisplayName => "DiningTable";
    public Transform InteractPoint => _interactPoint;
    private List<Plate> _currentPlates = new List<Plate>();

    void Awake()
    {
        for (int i = 0; i < _seats.Length; i++)
        {
            _seats[i].Init(this, i);
            _currentPlates.Add(null);
        }
    }

    public void PlacePlate(int seatIndex, Plate plate)
    {
        _currentPlates[seatIndex] = plate;
        plate.transform.SetParent(transform);
        plate.transform.position = _plateSlots[seatIndex].position;
    }

    public DiningSeat GetAvailableSeat()
    {
        return _seats.FirstOrDefault(seat => !seat.HasCustomer);
    }


    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        Debug.Log($"[DiningTable] InteractAsync START - character: {character.name}, IsHolding: {character.IsHolding}");

        if (character.IsHolding)
        {
            Debug.Log("[DiningTable] Branch: Serving food...");
            await ServeFood(character);
        }
        else
        {
            Debug.Log("[DiningTable] Branch: Picking up plate...");
            await PickUpPlate(character);
        }

        Debug.Log("[DiningTable] InteractAsync END");
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

                // 손님이 음식 먹고 나가기 (접시를 넘겨줌)
                customer.EatAndLeave(plate);
                return;
            }
        }
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
