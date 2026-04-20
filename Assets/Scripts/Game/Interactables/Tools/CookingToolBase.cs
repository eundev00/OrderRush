using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using JetBrains.Annotations;

public abstract class CookingToolBase : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [NotNull][SerializeField] protected Transform _interactPoint;
    [NotNull][SerializeField] protected Transform _ingredientSlot;
    [NotNull][SerializeField] protected Canvas _canvas;
    [NotNull][SerializeField] protected CookingProgressView _progressView;

    protected IngredientObject _currentIngredientObject;

    public abstract string DisplayName { get; }
    public Transform InteractPoint => _interactPoint;
    public IngredientData CurrentIngredientData => _currentIngredientObject != null ? _currentIngredientObject.Data : null;
    public bool HasIngredient => _currentIngredientObject != null;

    public bool IsCooking { get; protected set; }
    private SpawnFactory _factory;

    protected virtual void Awake()
    {
        _canvas.worldCamera = Camera.main;
    }


    [Inject]
    public void Construct(SpawnFactory factory)
    {
        _factory = factory;
    }

    protected virtual void OnDestroy()
    {
        StopCooking();
    }

    public virtual void PlaceIngredient(IngredientData ingredient, IngredientObject ingredientObject)
    {
        if (HasIngredient)
        {
            Debug.LogWarning($"[{DisplayName}] 이미 재료가 있습니다.");
            return;
        }

        _currentIngredientObject = ingredientObject;
        ingredientObject.SetData(ingredient);
        Debug.Log($"[{DisplayName}] 재료 배치: {ingredient.IngredientName}");
    }

    public virtual void RemoveIngredient()
    {
        if (!HasIngredient)
        {
            Debug.LogWarning($"[{DisplayName}] 재료가 없습니다.");
            return;
        }

        var ingredientData = _currentIngredientObject.Data;
        _currentIngredientObject = null;
        StopCooking();
        Debug.Log($"[{DisplayName}] 재료 제거: {ingredientData.IngredientName}");

    }


    protected virtual bool CanPlaceIngredient(IngredientData ingredient)
    {
        return true;
    }


    protected virtual async void StartCooking()
    {
        await UniTask.CompletedTask;
    }

    protected virtual void StopCooking()
    {
        _progressView.SetVisible(false);
        IsCooking = false;
    }

    protected void UpdateProgress(float elapsedTime, float duration)
    {
        float progress = elapsedTime / duration;
        _progressView.SetProgress(progress);
    }


    public virtual async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        Debug.Log($"[CookingToolBase] InteractAsync 호출됨 - IsHolding: {character.IsHolding}, IsOccupied: {HasIngredient}");

        // 뭔가를 들고 있을 때
        if (character.IsHolding)
        {
            // 1. 접시를 들고 있고 재료가 있으면 → 접시에 재료 올리기
            if (character.CurrentCarriable is Plate plate && HasIngredient)
            {
                character.PickUp(_currentIngredientObject);
                await plate.Stack(_currentIngredientObject, character, ct);
                character.PickUp(plate);
                Debug.Log($"[{DisplayName}] 접시에 재료 올림: {_currentIngredientObject.Data.IngredientName}");
                RemoveIngredient();
            }
            // 2. 재료를 들고 있고 재료가 없으면 → 재료 올리기
            else if (character.CurrentCarriable is IngredientObject ingredientObj && !HasIngredient)
            {
                if (CanPlaceIngredient(ingredientObj.Data))
                {
                    character.PutDown();
                    ingredientObj.transform.SetParent(_ingredientSlot);
                    ingredientObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    PlaceIngredient(ingredientObj.Data, ingredientObj);
                    StartCooking();
                    Debug.Log($"[{DisplayName}] 재료 올림: {ingredientObj.Data.IngredientName}");
                }
                else
                {
                    Debug.LogWarning($"[{DisplayName}] 이 도구에 올릴 수 없는 재료입니다.");
                }
            }
        }
        // 빈손일 때
        else
        {
            // 재료가 있으면 → 재료 집기
            if (HasIngredient)
            {
                character.PickUp(_currentIngredientObject);
                RemoveIngredient();
                Debug.Log($"[{DisplayName}] 재료 집음: {_currentIngredientObject.Data.IngredientName}");
            }
        }

        Debug.Log($"[{DisplayName}] IsCooking: {IsCooking}, IsOccupied: {HasIngredient}");

        await UniTask.CompletedTask;
    }

    protected async UniTask CompleteTransition(IngredientTransition transition)
    {
        Destroy(_currentIngredientObject.gameObject);
        _currentIngredientObject = await _factory.Create<IngredientObject>(PrefabKeys.GetPrefabPath(transition.Result.PrefabName));
        _currentIngredientObject.SetData(transition.Result);
        _currentIngredientObject.transform.SetParent(_ingredientSlot);
        _currentIngredientObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _currentIngredientObject.transform.localScale = Vector3.one;

    }
}
