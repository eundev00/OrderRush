using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class KitchenTable : MonoBehaviour, IInteractable
{
    [SerializeField] Transform _interactPoint;
    [SerializeField] Transform _slot;
    [SerializeField] Plate _initialPlate;

    ICarriable _carriable;

    public string DisplayName => "Kitchen Table";
    public Transform InteractPoint => _interactPoint;

    void Awake()
    {
        if (_initialPlate != null)
        {
            _carriable = _initialPlate;
            _carriable.OnPutDown(_slot);
        }
    }

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null)
        {
            Debug.LogWarning("[KitchenTable] Character is null");
            return;
        }

        if (character.IsHolding)
        {
            var carriable = character.CurrentCarriable;
            if (_carriable is IStackable stackable && stackable.CanStack(carriable))
            {
                var item = character.PutDown();
                await stackable.Stack(item, character, ct);
            }
            else if (_carriable == null && carriable is ICarriable)
            {
                var item = character.PutDown();
                _carriable = item;
                _carriable.OnPutDown(_slot);
                Debug.Log($"[KitchenTable] {item.GetType().Name} placed on table");
            }
            else
            {
                Debug.Log("[KitchenTable] Table already has an item");
            }
        }
        else
        {
            if (_carriable == null)
            {
                return;
            }

            character.PickUp(_carriable);  // OnPickedUp 자동 호출됨
            _carriable = null;
        }

        await UniTask.CompletedTask;
    }
}
