using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TrashCan : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform _interactPoint;

    public string DisplayName => "TrashCan";
    public Transform InteractPoint => _interactPoint;

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (!character.IsHolding) return;

        if (character.PutDown() is MonoBehaviour item)
        {
            await UniTask.Delay(100, cancellationToken: ct);
            Destroy(item.gameObject);
        }

        await UniTask.CompletedTask;
    }
}