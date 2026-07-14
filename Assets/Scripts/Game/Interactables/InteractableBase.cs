using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected InteractableHighlight _highlight;
    [NotNull][SerializeField] protected Transform[] _interactPoints;



    public Transform[] GetInteractPointsSortedByDistance(Vector3 fromPosition)
    {
        return _interactPoints
            .OrderBy(p => Vector3.Distance(fromPosition, p.position))
            .ToArray();
    }

    virtual public UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }

    public void SetHighlight(bool highlight)
    {
        if (_highlight) _highlight.SetSelected(highlight);
    }



}
