using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

public class Refrigerator : InteractableBase
{
    [NotNull][SerializeField] IngredientData _ingredient;
    [NotNull][SerializeField] Transform _doorTransform;

    [SerializeField] int _quantity = -1;  // -1이면 무한

    public bool IsEmpty => _currentPlateIndex == 0;
    private SpawnFactory _factory;
    private int _currentPlateIndex;
    private bool _isDoorOpen = false;

    [Inject]
    public void Construct(SpawnFactory factory)
    {
        _factory = factory;
        _currentPlateIndex = _quantity;
    }

    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null)
            return;

        if (character.IsHolding)
            return;

        if (IsEmpty)
            return;

        if (_ingredient == null)
        {
            Debug.LogError("IngredientData or Prefab is not assigned!");
            return;
        }

        await OpenDoorAnimation();

        if (!IsEmpty)
        {
            // IngredientObject 생성
            var ingredientObject = await _factory.Create<IngredientObject>(PrefabKeys.GetPrefabPath(_ingredient.PrefabName));
            ingredientObject.SetData(_ingredient);
            ingredientObject.transform.SetParent(character.ItemSlot);
            ingredientObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // 캐릭터에게 전달
            await character.PickUp(ingredientObject, ct);

            // 수량 감소 (-1이면 무한)
            if (_quantity > 0)
            {
                _currentPlateIndex--;
            }
        }

    }

    private UniTask OpenDoorAnimation()
    {
        _isDoorOpen = true;
        _doorTransform.DOKill();
        var tcs = new UniTaskCompletionSource();
        _doorTransform.DOLocalRotate(new Vector3(0, -90, 0), 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => tcs.TrySetResult());
        return tcs.Task;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && _isDoorOpen)
        {
            CloseDoorAnimation();
        }
    }



    private void CloseDoorAnimation()
    {
        _isDoorOpen = false;
        _doorTransform.DOKill();
        _doorTransform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.OutBack);
    }



}
