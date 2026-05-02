using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TrashCan : InteractableBase
{
    [NotNull][SerializeField] Transform _slot;
    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (!character.IsHolding) return;

        var carriable = await character.PutDownAt(_slot);
        if (carriable != null)
        {
            var component = carriable as Component;
            Destroy(component.gameObject);
        }

    }
}