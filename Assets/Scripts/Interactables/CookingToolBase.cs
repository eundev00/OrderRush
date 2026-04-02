using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public abstract class CookingToolBase : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] protected Transform _interactPoint;
    [SerializeField] protected Transform _ingredientSlot;
    [SerializeField] protected Canvas _canvas;
    [SerializeField] protected CookingProgressView _progressView;

    protected IngredientObject _currentIngredientObject;

    private IngredientTransition _currentTransition;
    private CancellationTokenSource _cookingCts;
    private float _cookingElapsedTime;

    public abstract string DisplayName { get; }
    public Transform InteractPoint => _interactPoint;
    public IngredientData CurrentIngredient => _currentIngredientObject != null ? _currentIngredientObject.Data : null;
    public bool IsOccupied => _currentIngredientObject != null;
    public bool IsCooking => _cookingCts != null;

    protected virtual void Awake()
    {
        _canvas.worldCamera = Camera.main;
    }



    protected virtual void Start()
    {
    }

    protected virtual void OnDestroy()
    {
        StopCookingTimer();
    }

    public virtual void PlaceIngredient(IngredientData ingredient, IngredientObject ingredientObject)
    {
        if (IsOccupied)
        {
            Debug.LogWarning($"[{DisplayName}] 이미 재료가 있습니다.");
            return;
        }

        _currentIngredientObject = ingredientObject;
        ingredientObject.SetData(ingredient);
        Debug.Log($"[{DisplayName}] 재료 배치: {ingredient.IngredientName}");
    }

    public virtual IngredientData RemoveIngredient()
    {
        if (!IsOccupied)
        {
            Debug.LogWarning($"[{DisplayName}] 재료가 없습니다.");
            return null;
        }

        var ingredientData = _currentIngredientObject.Data;
        _currentIngredientObject = null;
        StopCookingTimer();
        Debug.Log($"[{DisplayName}] 재료 제거: {ingredientData.IngredientName}");
        return ingredientData;
    }

    protected void StartCookingTimer(IngredientTransition transition)
    {
        if (_cookingCts != null)
        {
            StopCookingTimer();
        }

        _currentTransition = transition;
        _cookingCts = new CancellationTokenSource();
        _cookingElapsedTime = 0f;

        _progressView.SetVisible(true);
        _progressView.SetCookingStyle();
        CookAsync().Forget();
    }

    protected void StopCookingTimer()
    {
        Debug.Log($"[StopCookingTimer]초");
        if (_cookingCts != null)
        {
            _cookingCts.Cancel();
            _cookingCts.Dispose();
            _cookingCts = null;
            _currentTransition = null;
            _cookingElapsedTime = 0f;
            _progressView.SetVisible(false);
            _progressView.SetProgress(0f);
        }
    }

    public float GetProgress()
    {
        if (_currentTransition == null || _currentTransition.Duration <= 0)
            return 0f;

        return Mathf.Clamp01(_cookingElapsedTime / _currentTransition.Duration);
    }

    protected async UniTask CookAsync()
    {
        var ct = _cookingCts.Token;

        try
        {
            while (_cookingElapsedTime < _currentTransition.Duration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                _cookingElapsedTime += Time.deltaTime;
                _progressView.SetProgress(GetProgress());
            }

            if (IsOccupied)
            {
                var resultData = _currentTransition.Result;
                _currentIngredientObject.SetData(resultData);
                Debug.Log($"[{DisplayName}] 조리 완료: {resultData.IngredientName}");
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"[CookAsync]: 취소됨");
        }
        finally
        {
            _cookingCts?.Dispose();
            _cookingCts = null;
            _currentTransition = null;
            _cookingElapsedTime = 0f;
            _progressView.SetVisible(false);
            _progressView.SetProgress(0f);
        }
    }

    public abstract UniTask InteractAsync(CharacterBase character, CancellationToken ct);
}
