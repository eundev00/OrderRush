using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class StorageBase : MonoBehaviour, IInteractable, IInjectable
{
    [SerializeField] string _displayName = "Storage";
    [SerializeField] IngredientData _ingredient;
    [SerializeField] int _quantity = -1; // -1이면 무한
    [SerializeField] Transform _interactPoint;

    public string DisplayName => _displayName;
    public Transform InteractPoint => _interactPoint;
    public bool IsEmpty => _quantity == 0;
    private GameObjectFactory _factory;

    [Inject]
    public void Construct(GameObjectFactory factory)
    {
        _factory = factory;
    }

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null)
        {
            Debug.LogWarning($"[{DisplayName}] Character is null");
            return;
        }

        // 이미 들고 있으면 무시
        if (character.IsHolding)
        {
            Debug.Log($"[{DisplayName}] {character.name} is already holding an item");
            return;
        }

        // 재료가 없으면 무시
        if (IsEmpty)
        {
            Debug.Log($"[{DisplayName}] No ingredients available");
            return;
        }

        // Prefab 유효성 검사
        if (_ingredient == null)
        {
            Debug.LogError($"[{DisplayName}] IngredientData or Prefab is not assigned!");
            return;
        }

        // 재료를 꺼내는 애니메이션 시간 시뮬레이션
        await UniTask.Delay(500, cancellationToken: ct);

        // IngredientObject 생성
        var ingredientObject = await _factory.CreateAsync<IngredientObject>(PrefabKeys.GetPrefabPath(_ingredient.PrefabName));
        ingredientObject.SetData(_ingredient);
        ingredientObject.transform.SetParent(character.ItemSlot);
        ingredientObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        ingredientObject.transform.localScale = Vector3.one;


        // 캐릭터에게 전달
        character.PickUp(ingredientObject);

        // 수량 감소 (-1이면 무한)
        if (_quantity > 0)
        {
            _quantity--;
        }

        string remaining = _quantity == -1 ? "Infinite" : _quantity.ToString();
        Debug.Log($"[{DisplayName}] {character.name} took: {_ingredient.IngredientName} (Remaining: {remaining})");
    }

    // Inspector에서 재료 개수 확인용
    public int GetIngredientCount() => _quantity;
}
