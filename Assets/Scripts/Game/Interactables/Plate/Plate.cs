using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Plate : MonoBehaviour, ICarriable, IStackable
{
    [SerializeField] Transform _ingredientSlot;

    List<IngredientObject> _placedIngredients = new();

    [Inject] IOrderService _orderService;

    public string DisplayName => "Plate";

    public void OnPickedUp(Transform slot)
    {
        transform.SetParent(slot);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;
    }

    public void OnPutDown(Transform slot)
    {
        transform.SetParent(slot);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;
    }

    public bool CanStack(ICarriable item) => item is IngredientObject;

    public async UniTask Stack(ICarriable item, CharacterBase character, CancellationToken ct)
    {
        if (item is IngredientObject ingredientObj)
        {
            ingredientObj.OnPutDown(_ingredientSlot);
            _placedIngredients.Add(ingredientObj);
            Debug.Log($"[Plate] Ingredient stacked: {ingredientObj.Data.IngredientName}");
            CheckRecipe();
        }

        await UniTask.CompletedTask;
    }

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;
        if (!character.IsHolding) return;

        var carriable = character.CurrentCarriable;
        if (CanStack(carriable))
        {
            character.PutDown();
            await Stack(carriable, character, ct);
        }

        await UniTask.CompletedTask;
    }

    void CheckRecipe()
    {
        var orders = _orderService.GetActiveOrders();
        var ingredientDatas = _placedIngredients.Select(obj => obj.Data).ToList();

        foreach (var order in orders)
        {
            if (order.Recipe.IsComplete(ingredientDatas))
            {
                Debug.Log($"[Plate] Recipe matched: {order.Recipe.RecipeName}");
                _orderService.CompleteOrder(order);
                break;
            }
        }
    }
}
