using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class PlateRack : InteractableBase
{
    [NotNull][SerializeField] GameObject[] _plates;
    [SerializeField] int _quantity = 4;

    private SpawnFactory _factory;
    private int _currentPlateIndex;

    [Inject]
    public void Construct(SpawnFactory factory)
    {
        _factory = factory;
    }

    void Start()
    {
        _currentPlateIndex = _quantity;
        foreach (var plate in _plates)
        {
            plate.SetActive(true);
        }
    }

    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;

        // 1. 깨끗한 접시를 들고 있는 경우 → 렉에 반환
        if (character.IsHolding && character.CurrentCarriable is Plate heldPlate && !heldPlate.IsDirty)
        {
            if (_currentPlateIndex < _quantity)
            {
                await character.PutDown();
                Destroy(heldPlate.gameObject);
                _currentPlateIndex++;
                RestorePlateCount();
            }
            return;
        }

        // 2. 렉이 비어있으면 아무것도 안 함
        if (_currentPlateIndex <= 0)
        {
            return;
        }

        // 3. 재료를 들고 있는 경우 → 접시 꺼내서 재료 올리고 들기
        if (character.IsHolding)
        {
            if (character.CurrentCarriable.GetCarriableType() == CarriableType.Ingredient)
            {
                var plate = await GetNewPlate();
                if (plate.TryPlaceOntoOther(character.CurrentCarriable))
                {
                    await character.PickUp(plate);
                    _currentPlateIndex--;
                    UpdatePlateCount();
                }
                else
                {
                    Destroy(plate.gameObject);
                }
            }
        }
        // 4. 빈 손인 경우 → 접시 꺼내서 들기
        else
        {
            var plate = await GetNewPlate();
            if (plate != null)
            {
                await character.PickUp(plate);
                _currentPlateIndex--;
                UpdatePlateCount();
            }
        }

        await UniTask.CompletedTask;
    }

    private async UniTask<Plate> GetNewPlate()
    {
        return await _factory.Create<Plate>(PrefabKeys.GetPrefabPath(PrefabKeys.Plate));

    }

    void UpdatePlateCount()
    {
        if (_plates.Length <= _currentPlateIndex)
        {
            return;
        }
        _plates[_currentPlateIndex].SetActive(false);
    }

    void RestorePlateCount()
    {
        if (_currentPlateIndex > 0 && _currentPlateIndex <= _plates.Length)
        {
            _plates[_currentPlateIndex - 1].SetActive(true);
        }
    }
}
