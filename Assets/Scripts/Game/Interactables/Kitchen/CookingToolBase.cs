using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public abstract class CookingToolBase : InteractableBase
{

    [NotNull][SerializeField] protected Transform _ingredientSlot;

    public IngredientObject CurrentIngredientObject { get; protected set; }
    public IngredientData CurrentIngredientData => CurrentIngredientObject != null ? CurrentIngredientObject.Data : null;
    public bool HasIngredient => CurrentIngredientObject != null;

    public bool IsCooking { get; protected set; }
    private SpawnFactory _factory;
    protected GaugeFactory _gaugeFactory;
    protected GaugePresenter _gaugePresenter;

    [Inject]
    public void Construct(SpawnFactory factory, GaugeFactory gaugeFactory)
    {
        _factory = factory;
        _gaugeFactory = gaugeFactory;
    }

    protected virtual void OnDestroy()
    {
        RemoveIngredient();
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
            CurrentIngredientObject = null;
            StopCooking();
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
            _gaugePresenter.UpdatePosition();
            _gaugePresenter.SetProgress(progress);
        }
    }

    protected void ShowCookingGauge()
    {
        if (_gaugePresenter == null)
        {
            _gaugePresenter = _gaugeFactory.Create(transform, new Vector3(0, 0.5f, 0));
            _gaugePresenter.Show();
            _gaugePresenter.SetColor(Color.green);
        }
        else
        {
            _gaugePresenter.Show();
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
