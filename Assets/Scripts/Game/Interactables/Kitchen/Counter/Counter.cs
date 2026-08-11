using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Counter : InteractableBase
{
    [NotNull][SerializeField] Transform _slot;

    ICarriable _placedCarriable;

    public event Action<Counter> ItemPlaced;

    public bool HasItem => _placedCarriable != null;
    public ICarriable CurrentItem => _placedCarriable;

    protected override void OnGameCleanup()
    {
        _placedCarriable.DestroyObject();
        _placedCarriable = null;
    }

    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;

        if (character.IsHolding && _placedCarriable != null)
        {
            if (character.CurrentCarriable.GetCarriableType() == CarriableType.Plate)
            {
                var plate = character.CurrentCarriable as Plate;
                if (plate.TryPlaceOntoOther(_placedCarriable))
                {
                    await character.PickUp(character.CurrentCarriable, ct);
                    _placedCarriable = null;
                }
            }
            else if (_placedCarriable.GetCarriableType() == CarriableType.Plate)
            {
                var plate = _placedCarriable as Plate;
                if (plate.TryPlaceOntoOther(character.CurrentCarriable))
                {
                    await character.PutDown(ct);
                }
            }
        }
        else if (character.IsHolding && _placedCarriable == null)
        {
            _placedCarriable = await character.PutDownAt(_slot, ct);
            ItemPlaced?.Invoke(this);
        }
        else if (!character.IsHolding && _placedCarriable != null)
        {
            await character.PickUp(_placedCarriable, ct);
            _placedCarriable = null;
        }
    }
}
