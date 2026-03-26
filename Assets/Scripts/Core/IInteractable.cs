using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IInteractable
{
    string DisplayName { get; }
    Transform InteractPoint { get; }
    UniTask InteractAsync(CharacterBase character, CancellationToken ct);
}