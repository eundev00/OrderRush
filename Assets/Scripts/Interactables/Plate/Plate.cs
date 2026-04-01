using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Plate : MonoBehaviour, ICarriable
{
    [SerializeField] Transform _ingredientSlot;

    List<IngredientObject> _placedIngredients = new();

    [Inject] IOrderService _orderService;

    public string DisplayName => "Plate";

    public void OnPlaced(Transform slot)
    {
        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void OnPickedUp(Transform slot)
    {
        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public bool CanReceive(ICarriable item) => item is IngredientObject;

    public async UniTask Receive(ICarriable item, CharacterBase character, CancellationToken ct)
    {
        if (item is IngredientObject ingredientObj)
        {
            ingredientObj.OnPlaced(_ingredientSlot);
            _placedIngredients.Add(ingredientObj);
            Debug.Log($"[Plate] Ingredient placed: {ingredientObj.Context}");
            CheckRecipe();
        }

        await UniTask.CompletedTask;
    }

    public async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (character == null) return;

        if (!character.IsHolding) return;

        var carriable = character.CurrentCarriable;
        if (CanReceive(carriable))
        {
            character.PutDown();
            await Receive(carriable, character, ct);
        }

        await UniTask.CompletedTask;
    }

    void CheckRecipe()
    {
        var orders = _orderService.GetActiveOrders();

        foreach (var order in orders)
        {
            if (IsRecipeComplete(order.Recipe))
            {
                Debug.Log($"[Plate] Recipe matched: {order.Recipe.RecipeName}");

                // 재료들 제거
                foreach (var ingredient in _placedIngredients)
                {
                    Destroy(ingredient.gameObject);
                }
                _placedIngredients.Clear();

                // ResultItem 생성
                if (order.Recipe.ResultItem != null && order.Recipe.ResultItem.Prefab != null)
                {
                    var resultObj = Instantiate(order.Recipe.ResultItem.Prefab, _ingredientSlot);
                    var resultIngredient = resultObj.GetComponent<IngredientObject>();
                    resultIngredient.Initialize(order.Recipe.ResultItem);
                    _placedIngredients.Add(resultIngredient);

                    Debug.Log($"[Plate] Result item created: {order.Recipe.ResultItem.IngredientName}");
                }

                // 주문 완료
                _orderService.CompleteOrder(order);
                break;
            }
        }
    }

    bool IsRecipeComplete(RecipeData recipe)
    {
        // 레시피의 모든 재료가 충족되는지 확인
        foreach (var required in recipe.Ingredients)
        {
            bool found = _placedIngredients.Any(placed =>
                placed.Context.Data == required.Data &&
                placed.Context.State == required.RequiredState);

            if (!found)
            {
                return false;
            }
        }

        // 재료 개수도 정확히 일치해야 함
        return _placedIngredients.Count == recipe.Ingredients.Count;
    }
}
