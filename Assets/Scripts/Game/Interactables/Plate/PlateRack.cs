using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class PlateRack : MonoBehaviour, IInteractable
{
    [NotNull][SerializeField] Transform _interactPoint;
    [SerializeField] int _quantity = 5;
    [NotNull][SerializeField] Transform _plateVisual;
    [SerializeField] float _heightPerPlate = 0.2f;

    private SpawnFactory _factory;


    public string DisplayName => "Plate Rack";
    public Transform InteractPoint => _interactPoint;
    private Vector3 _basePosition;

    [Inject]
    public void Construct(SpawnFactory factory)
    {
        _factory = factory;
    }

    void Start()
    {
        _basePosition = _plateVisual.localPosition;
        UpdatePlateHeight();
    }

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;

        // 수량이 0이면 무시
        if (_quantity <= 0)
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
                _quantity--;
                UpdatePlateHeight();
                Debug.Log($"[PlateRack] Ingredient placed on new plate. Remaining plates: {_quantity}");
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
            _quantity--;
            UpdatePlateHeight();
            Debug.Log($"[PlateRack] Plate picked up. Remaining: {_quantity}");
        }

        await UniTask.CompletedTask;
    }

    void UpdatePlateHeight()
    {
        if (_plateVisual == null) return;

        // 수량이 0이면 숨기기
        if (_quantity <= 0)
        {
            _plateVisual.gameObject.SetActive(false);
            return;
        }

        // 수량에 따라 높이 조정
        _plateVisual.gameObject.SetActive(true);
        _plateVisual.localPosition = _basePosition + Vector3.up * (_quantity * _heightPerPlate);
        Debug.Log($"[PlateRack] UpdatePlateHeight - Quantity: {_quantity}, Position: {_plateVisual.localPosition}");
    }
}
