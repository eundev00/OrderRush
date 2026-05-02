using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class KitchenTable : InteractableBase
{

    [NotNull][SerializeField] Transform _slot;
    [SerializeField] Plate _initialPlate;

    ICarriable _placedCarriable;

    void Awake()
    {
        if (_initialPlate != null)
        {
            _placedCarriable = _initialPlate;
            _placedCarriable.AttachToSlot(_slot);
        }
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
                    await character.PickUp(character.CurrentCarriable);
                    _placedCarriable = null;
                }
            }
            else if (_placedCarriable.GetCarriableType() == CarriableType.Plate)
            {
                var plate = _placedCarriable as Plate;
                if (plate.TryPlaceOntoOther(character.CurrentCarriable))
                {
                    await character.PutDown();
                }
            }

        }
        else if (character.IsHolding && _placedCarriable == null)
        {
            _placedCarriable = await character.PutDownAt(_slot);
        }
        else if (character.IsHolding == false && _placedCarriable != null)
        {
            await character.PickUp(_placedCarriable);
            _placedCarriable = null;
        }


        await UniTask.CompletedTask;
    }
}
