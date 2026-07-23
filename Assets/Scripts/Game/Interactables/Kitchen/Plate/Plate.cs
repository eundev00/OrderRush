using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Plate : MonoBehaviour, ICarriable
{
    [NotNull][SerializeField] Transform _ingredientSlot;
    [NotNull][SerializeField] GameObject _dirty;

    private List<IngredientObject> _placedIngredients = new();
    private IGameDataService _gameDataService;

    public List<IngredientObject> PlacedIngredients => _placedIngredients;
    public bool IsDirty { get; private set; }
    public int MatchedRecipeID { get; private set; } = -1;

    [Inject]
    public void Construct(IGameDataService gameDataService)
    {
        _gameDataService = gameDataService;
    }

    private void Awake()
    {
        SetClean();
    }

    public void AttachToSlot(Transform slot)
    {
        transform.SetParent(slot);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public CarriableType GetCarriableType()
    {
        return CarriableType.Plate;
    }

    public bool TryPlaceOntoOther(ICarriable other)
    {
        var ingredientObj = other as IngredientObject;
        if (ingredientObj == null) return false;

        if (_placedIngredients.Any(i => i.Data == ingredientObj.Data)) return false;

        ingredientObj.OnPutDown(_ingredientSlot);
        _placedIngredients.Add(ingredientObj);
        IsDirty = true;
        UpdateMatchedRecipeID();
        return true;
    }

    public void RemoveEatenFood()
    {
        foreach (var ingredient in _placedIngredients)
            Destroy(ingredient.gameObject);

        _placedIngredients.Clear();
        MatchedRecipeID = -1;
        _dirty.SetActive(true);
    }

    public void SetClean()
    {
        IsDirty = false;
        MatchedRecipeID = -1;
        _dirty.SetActive(false);
    }


    private void UpdateMatchedRecipeID()
    {
        if (_gameDataService == null) return;
        var ingredientDatas = _placedIngredients.Select(i => i.Data).ToList();
        MatchedRecipeID = _gameDataService.GetMatchedRecipeID(ingredientDatas);
    }
}
