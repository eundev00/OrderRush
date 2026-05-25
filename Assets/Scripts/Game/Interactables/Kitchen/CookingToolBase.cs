using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Services;
using UnityEngine;
using VContainer;

public abstract class CookingToolBase : InteractableBase
{

    [NotNull][SerializeField] protected Transform _ingredientSlot;

    public IngredientObject CurrentIngredientObject { get; protected set; }

    public bool IsCooking { get; protected set; }
    private SpawnFactory _factory;
    protected KitchenGaugeFactory _gaugeFactory;
    protected KitchenGaugePresenter _gaugePresenter;
    protected IGameDataService _gameDataService;
    private IDisposable _gameCleanupSubscription;
    private IDisposable _dayEndedSubscription;

    public IngredientData CurrentIngredientData => CurrentIngredientObject != null ? CurrentIngredientObject.Data : null;
    public bool HasIngredient => CurrentIngredientObject != null;

    [Inject]
    public void Construct(SpawnFactory factory, KitchenGaugeFactory gaugeFactory, IGameDataService gameDataService, ISubscriber<GameCleanupEvent> gameCleanupSubscriber, ISubscriber<DayEndedEvent> dayEndedSubscriber)
    {
        _factory = factory;
        _gaugeFactory = gaugeFactory;
        _gameDataService = gameDataService;
        _gameCleanupSubscription = gameCleanupSubscriber.Subscribe(_ => OnGameCleanup());
        _dayEndedSubscription = dayEndedSubscriber.Subscribe(_ => OnDayEnded());
    }

    protected virtual void OnDestroy()
    {
        RemoveIngredient();
        _gameCleanupSubscription?.Dispose();
        _dayEndedSubscription?.Dispose();
    }

    protected virtual void OnDayEnded()
    {
        StopCooking();
    }

    protected virtual void OnGameCleanup()
    {
        StopCooking();
        CurrentIngredientObject = null;
    }

    public virtual void PlaceIngredient(IngredientData ingredient, IngredientObject ingredientObject)
    {
        if (HasIngredient)
        {
            Debug.LogWarning("이미 재료가 있습니다.");
            return;
        }

        Debug.Log($"[CookingToolBase] PlaceIngredient - Ingredient: {(ingredient != null ? ingredient.IngredientName : "null")}, IngredientObject: {ingredientObject}");
        CurrentIngredientObject = ingredientObject;
        ingredientObject.SetData(ingredient);
    }

    public virtual void RemoveIngredient()
    {
        if (HasIngredient)
        {
            Debug.Log($"[CookingToolBase] RemoveIngredient - Removing: {CurrentIngredientObject} (Data: {CurrentIngredientData})");
            StopCooking();
            CurrentIngredientObject = null;
        }
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
        HideCookingGauge();
        IsCooking = false;
    }

    protected void UpdateProgress(float progress)
    {
        if (_gaugePresenter != null)
        {
            _gaugePresenter.SetProgress(progress);
        }
    }

    protected void ShowCookingGauge()
    {
        if (_gaugePresenter == null)
        {
            _gaugePresenter = _gaugeFactory.Create(transform, new Vector3(0, 0.5f, 0));
            _gaugePresenter.Show();
            _gaugePresenter.SetProgress(0f);
        }
        else
        {
            _gaugePresenter.Show();
            _gaugePresenter.SetProgress(0f);
        }
    }

    protected void HideCookingGauge()
    {
        if (_gaugePresenter != null)
        {
            _gaugeFactory.Release(_gaugePresenter);
            _gaugePresenter = null;
        }
    }


    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {

        if (character.IsHolding && HasIngredient)
        {
            if (character.CurrentCarriable.GetCarriableType() == CarriableType.Plate)
            {
                var plate = character.CurrentCarriable as Plate;
                if (plate.TryPlaceOntoOther(CurrentIngredientObject))
                {
                    await character.PickUp(character.CurrentCarriable);
                    RemoveIngredient();
                }
            }

        }
        else if (character.IsHolding && !HasIngredient)
        {
            var ingredientObj = character.CurrentCarriable as IngredientObject;
            if (ingredientObj && CanPlaceIngredient(ingredientObj.Data))
            {
                await character.PutDownAt(_ingredientSlot);
                CurrentIngredientObject = ingredientObj;
                PlaceIngredient(CurrentIngredientObject.Data, CurrentIngredientObject);
                StartCooking();
            }
            else
            {
                Debug.LogWarning("이 도구에 올릴 수 없는 재료입니다.");
            }
        }
        else if (!character.IsHolding && HasIngredient)
        {
            StopCooking();
            await character.PickUp(CurrentIngredientObject);
            RemoveIngredient();
        }

        await UniTask.CompletedTask;
    }

    protected async UniTask CompleteTransition(IngredientTransition transition)
    {
        Debug.Log($"[CookingToolBase] CompleteTransition - Before: {CurrentIngredientObject}, Transition to: {transition.Result.IngredientName}");
        Destroy(CurrentIngredientObject.gameObject);
        CurrentIngredientObject = await _factory.Create<IngredientObject>(PrefabKeys.GetPrefabPath(transition.Result.PrefabName));
        CurrentIngredientObject.SetData(transition.Result);
        CurrentIngredientObject.transform.SetParent(_ingredientSlot);
        CurrentIngredientObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        Debug.Log($"[CookingToolBase] CompleteTransition - After: {CurrentIngredientObject} (Data: {CurrentIngredientData})");

    }
}
