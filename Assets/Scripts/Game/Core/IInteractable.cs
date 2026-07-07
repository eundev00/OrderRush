using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IInteractable
{
    Transform[] GetInteractPointsSortedByDistance(Vector3 fromPosition);
    UniTask InteractAsync(CharacterBase character, CancellationToken ct);
    void SetHighlight(bool highlight);
}