using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] Transform _interactPoint;
    public string DisplayName => gameObject.name;
    public Transform InteractPoint => _interactPoint;

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        Debug.Log($"[{DisplayName}] {character.name} interaction started");
        await UniTask.Delay(1000, cancellationToken: ct);
        Debug.Log($"[{DisplayName}] {character.name} interaction completed");
    }
}
