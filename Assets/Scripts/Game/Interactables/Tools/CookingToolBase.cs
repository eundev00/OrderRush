using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public abstract class CookingToolBase : MonoBehaviour, IInteractable, IInjectable
{
    [Header("Interaction")]
    [SerializeField] protected Transform _interactPoint;
    [SerializeField] protected Transform _ingredientSlot;
    [SerializeField] protected Canvas _canvas;
    [SerializeField] protected CookingProgressView _progressView;

    protected IngredientObject _currentIngredientObject;

    public abstract string DisplayName { get; }
    public Transform InteractPoint => _interactPoint;
    public IngredientData CurrentIngredientData => _currentIngredientObject != null ? _currentIngredientObject.Data : null;
    public bool HasIngredient => _currentIngredientObject != null;

    public bool IsCooking { get; protected set; }
    private GameObjectFactory _factory;

    protected virtual void Awake()
    {
        _canvas.worldCamera = Camera.main;
    }


    [Inject]
    public void Construct(GameObjectFactory factory)
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

        // 캐릭터가 아무것도 안 들고 있고 재료가 있으면 집기
        if (!character.IsHolding && HasIngredient)
        {
            character.PickUp(_currentIngredientObject);  // OnPickedUp 자동 호출됨
            Debug.Log($"[CookingToolBase] 재료 집음: {_currentIngredientObject.Data.IngredientName}");
            RemoveIngredient();
            return;
        }

        // 캐릭터가 재료 들고 있으면 올리기
        if (character.IsHolding && !HasIngredient)
        {
            Debug.Log($"[{DisplayName}] 재료 올리기 시도");
            var ingredientObject = character.CurrentCarriable as IngredientObject;
            if (ingredientObject != null && CanPlaceIngredient(ingredientObject.Data))
            {
                character.PutDown();
                ingredientObject.transform.SetParent(_ingredientSlot);
                ingredientObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                PlaceIngredient(ingredientObject.Data, ingredientObject);
                StartCooking();
            }
            else
            {
                Debug.LogWarning($"[{DisplayName}] 이 도구에 올릴 수 없는 재료입니다.");
            }
        }

        Debug.Log($"[Stove] IsCooking: {IsCooking}, IsOccupied: {HasIngredient}");


        await UniTask.CompletedTask;
    }

    protected async UniTask CompleteTransition(IngredientTransition transition)
    {
        Destroy(_currentIngredientObject.gameObject);
        _currentIngredientObject = await _factory.CreateAsync<IngredientObject>(PrefabKeys.GetPrefabPath(transition.Result.PrefabName));
        _currentIngredientObject.SetData(transition.Result);
        _currentIngredientObject.transform.SetParent(_ingredientSlot);
        _currentIngredientObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _currentIngredientObject.transform.localScale = Vector3.one;

    }
}
