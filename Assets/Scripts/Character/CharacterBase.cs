using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [SerializeField] Transform _itemSlot;

    protected IHoldable _heldItem;
    public bool IsHolding => _heldItem != null;
    public Transform ItemSlot => _itemSlot;

    public virtual void PickUp(IHoldable item)
    {
        if (item == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Cannot pick up null item");
            return;
        }

        if (_heldItem != null)
        {
            Debug.LogWarning($"[{gameObject.name}] Already holding an item: {_heldItem}");
            return;
        }

        _heldItem = item;
        Debug.Log($"[{gameObject.name}] Picked up: {item}");
    }

    public virtual IHoldable PutDown()
    {
        if (_heldItem == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No item to put down");
            return null;
        }

        var item = _heldItem;
        _heldItem = null;
        Debug.Log($"[{gameObject.name}] Put down: {item}");
        return item;
    }
}
