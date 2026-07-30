using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

public abstract class CookingToolBase : InteractableBase
{

    [NotNull][SerializeField] protected Transform _ingredientSlot;
    [NotNull][SerializeField] protected CookingGauge _cookingGauge;

    public IngredientObject CurrentIngredientObject { get; protected set; }

    public bool IsCooking { get; protected set; }
    private SpawnFactory _factory;
    protected IGameDataService _gameDataService;
    private IDisposable _dayEndedSubscription;

    public IngredientData CurrentIngredientData => CurrentIngredientObject != null ? CurrentIngredientObject.Data : null;
    public bool HasIngredient => CurrentIngredientObject != null;

    [Inject]
    public void Construct(SpawnFactory factory, IGameDataService gameDataService, ISubscriber<DayEndedEvent> dayEndedSubscriber)
    {
        _factory = factory;
        _gameDataService = gameDataService;
        _dayEndedSubscription = dayEndedSubscriber.Subscribe(_ => OnDayEnded());
    }

    protected virtual void Awake()
    {
        _cookingGauge.Hide();
    }

    protected virtual void OnDestroy()
    {
        RemoveIngredient();
        _dayEndedSubscription?.Dispose();
    }

    protected virtual void OnDayEnded()
    {
        StopCooking();
    }

    protected override void OnGameCleanup()
    {
        StopCooking();

        if (CurrentIngredientObject != null)
            Destroy(CurrentIngredientObject.gameObject);
        CurrentIngredientObject = null;
    }

    public virtual void PlaceIngredient(IngredientData ingredient, IngredientObject ingredientObject)
    {
        if (HasIngredient)
        {
            Debug.LogWarning("이미 재료가 있습니다.");
            return;
        }

        CurrentIngredientObject = ingredientObject;
        ingredientObject.SetData(ingredient);
    }

    public virtual void RemoveIngredient()
    {
        if (HasIngredient)
        {
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
        _cookingGauge.Hide();
        IsCooking = false;
    }

    protected void UpdateProgress(float progress)
    {
        _cookingGauge?.SetProgress(progress);
    }

    protected void ShowCookingGauge()
    {
        _cookingGauge.Show();
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
            await UniTask.Yield();
            await character.PickUp(CurrentIngredientObject);
            RemoveIngredient();
        }

        await UniTask.CompletedTask;
    }

    protected async UniTask CompleteTransition(IngredientTransition transition)
    {
        Destroy(CurrentIngredientObject.gameObject);
        CurrentIngredientObject = await _factory.Create<IngredientObject>(PrefabKeys.GetPrefabPath(transition.Result.PrefabName));
        CurrentIngredientObject.SetData(transition.Result);
        CurrentIngredientObject.transform.SetParent(_ingredientSlot);
        CurrentIngredientObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

    }
}
