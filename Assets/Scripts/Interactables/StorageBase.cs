using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StorageBase : MonoBehaviour, IInteractable
{
    [SerializeField] string _displayName = "Storage";
    [SerializeField] IngredientData _ingredient;
    [SerializeField] int _quantity = -1; // -1이면 무한
    [SerializeField] Transform _interactPoint;

    public string DisplayName => _displayName;
    public Transform InteractPoint => _interactPoint;
    public bool IsEmpty => _quantity == 0;

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

        // 재료를 꺼내는 애니메이션 시간 시뮬레이션
        await UniTask.Delay(500, cancellationToken: ct);

        // IngredientObject 생성
        var ingredientObj = Instantiate(_ingredient.prefab, character.ItemSlot);
        var ingredientObject = ingredientObj.GetComponent<IngredientObject>();
        ingredientObject.Initialize(_ingredient);

        // 캐릭터에게 전달
        character.PickUp(ingredientObject);

        // 수량 감소 (-1이면 무한)
        if (_quantity > 0)
        {
            _quantity--;
        }

        string remaining = _quantity == -1 ? "Infinite" : _quantity.ToString();
        Debug.Log($"[{DisplayName}] {character.name} took: {_ingredient.ingredientName} (Remaining: {remaining})");
    }

    // Inspector에서 재료 개수 확인용
    public int GetIngredientCount() => _quantity;
}
