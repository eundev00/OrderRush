using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Plate : MonoBehaviour, ICarriable
{
    [NotNull][SerializeField] Transform _ingredientSlot;
    [NotNull][SerializeField] GameObject _dirty;
    List<IngredientObject> _placedIngredients = new();

    public List<IngredientObject> PlacedIngredients => _placedIngredients;
    public bool IsDirty { get; private set; }


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

        // 이미 같은 재료 있으면 거부
        if (_placedIngredients.Any(i => i.Data == ingredientObj.Data)) return false;

        ingredientObj.OnPutDown(_ingredientSlot);
        _placedIngredients.Add(ingredientObj);
        SetDirty();
        return true;
    }



    public void RemoveEatenFood()
    {
        foreach (var ingredient in _placedIngredients)
        {
            Destroy(ingredient.gameObject);
        }
        _placedIngredients.Clear();
        _dirty.SetActive(true);
    }

    private void SetDirty()
    {
        IsDirty = true;
        _dirty.SetActive(true);
    }

    public void SetClean()
    {
        IsDirty = false;
        _dirty.SetActive(false);
    }

}
