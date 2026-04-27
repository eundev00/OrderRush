using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class PlateRack : MonoBehaviour, IInteractable
{
    [NotNull][SerializeField] Transform _interactPoint;
    [SerializeField] int _quantity = 4;
    [NotNull][SerializeField] GameObject[] _plates;

    private SpawnFactory _factory;
    private int _currentPlateIndex;

    public string DisplayName => "Plate Rack";
    public Transform InteractPoint => _interactPoint;

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

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;

        // 수량이 0이면 무시
        if (_currentPlateIndex <= 0)
        {
            Debug.Log("[PlateRack] No plates available");
            return;
        }

        // 접시 생성
        var plate = await _factory.Create<Plate>(PrefabKeys.GetPrefabPath(PrefabKeys.Plate));
        if (plate == null) return;

        // 뭔가를 들고 있을 때
        if (character.IsHolding)
        {
            // 재료를 들고 있으면 → 접시에 재료 담고 접시 들기
            if (character.CurrentCarriable is IngredientObject ingredientObj)
            {
                character.PutDown();
                await plate.Stack(ingredientObj, character, ct);
                character.PickUp(plate);
                _currentPlateIndex--;
                UpdatePlateCount();
                Debug.Log($"[PlateRack] Ingredient placed on new plate. Remaining plates: {_currentPlateIndex}");
            }
            // 재료가 아닌 것을 들고 있으면 → 무시
            else
            {
                Destroy(plate.gameObject);
                Debug.Log("[PlateRack] Character is holding something that cannot be placed on a plate");
            }
        }
        // 빈손일 때
        else
        {
            // 접시만 들기
            character.PickUp(plate);
            _currentPlateIndex--;
            UpdatePlateCount();
            Debug.Log($"[PlateRack] Plate picked up. Remaining: {_currentPlateIndex}");
        }

        await UniTask.CompletedTask;
    }

    void UpdatePlateCount()
    {
        if (_plates.Length <= _currentPlateIndex)
        {
            return;
        }
        _plates[_currentPlateIndex].SetActive(false);
    }
}
