using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// 조리 도구 베이스 클래스 (Pan, Oven, Grill 등)
/// </summary>
public abstract class CookingToolBase : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] protected Transform _interactPoint;
    [SerializeField] protected Transform _ingredientSlot;

    [Header("Debug")]
    [SerializeField] protected IngredientData _initialIngredient;

    [Inject] protected ICookingService _cookingService;

    protected IngredientContext _currentIngredient;
    private CookingProcess _cookingProcess;

    public event Action<CookingToolBase, CookingStep> OnCookingCompleted;

    public abstract string DisplayName { get; }
    public Transform InteractPoint => _interactPoint;
    public IngredientContext CurrentIngredient => _currentIngredient;
    public bool IsOccupied => _currentIngredient != null;
    public bool IsCooking => _cookingProcess != null;

    protected virtual void Awake()
    {
        if (_initialIngredient != null)
        {
            PlaceIngredient(_initialIngredient);
        }
    }

    protected virtual void Start()
    {
        _cookingService?.RegisterTool(this);
    }

    protected virtual void OnDestroy()
    {
        StopCookingTimer();
        _cookingService?.UnregisterTool(this);
    }

    public virtual void PlaceIngredient(IngredientData ingredient)
    {
        if (IsOccupied)
        {
            Debug.LogWarning($"[{DisplayName}] 이미 재료가 있습니다.");
            return;
        }

        _currentIngredient = new IngredientContext(ingredient);
        Debug.Log($"[{DisplayName}] 재료 배치: {_currentIngredient}");
    }

    public virtual IngredientContext RemoveIngredient()
    {
        if (!IsOccupied)
        {
            Debug.LogWarning($"[{DisplayName}] 재료가 없습니다.");
            return null;
        }

        var ingredientModel = _currentIngredient;
        _currentIngredient = null;
        StopCookingTimer();
        _cookingService?.CancelBurn(this);
        Debug.Log($"[{DisplayName}] 재료 제거: {ingredientModel}");
        return ingredientModel;
    }

    protected void StartCookingTimer(CookingStep step)
    {
        if (_cookingProcess != null)
        {
            StopCookingTimer();
        }

        _cookingProcess = new CookingProcess
        {
            Step = step,
            Cts = new CancellationTokenSource(),
            ElapsedTime = 0f
        };

        _cookingService?.StartCooking(this, step);
        CookAsync(_cookingProcess).Forget();
    }

    protected void StopCookingTimer()
    {
        if (_cookingProcess != null)
        {
            _cookingProcess.Cts?.Cancel();
            _cookingProcess.Cts?.Dispose();
            _cookingProcess = null;

            _cookingService?.StopCooking(this);
        }
    }

    public float GetProgress()
    {
        return _cookingProcess?.Progress ?? 0f;
    }

    private async UniTask CookAsync(CookingProcess process)
    {
        var ct = process.Cts.Token;
        var step = process.Step;

        try
        {
            while (process.ElapsedTime < step.duration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                process.ElapsedTime += Time.deltaTime;
            }

            OnCookingCompleted?.Invoke(this, step);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _cookingProcess = null;
        }
    }

    public abstract UniTask InteractAsync(CharacterBase character, CancellationToken ct);
}
