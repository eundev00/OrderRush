using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderService : IOrderService
{
    private readonly List<Order> _activeOrders = new();
    private const float DEFAULT_TIME_LIMIT = 60f;

    public Order AddOrder()
    {
        var recipe = GetRandomRecipe();
        if (recipe == null)
        {
            Debug.LogWarning("Cannot add order with null recipe");
            return null;
        }

        var order = new Order(recipe, DEFAULT_TIME_LIMIT);
        _activeOrders.Add(order);
        Debug.Log($"New order added: {recipe.RecipeName} (Time limit: {DEFAULT_TIME_LIMIT}s)!");
        return order;
    }

    public void CompleteOrder(Order order)
    {
        if (order == null)
        {
            Debug.LogWarning("Cannot complete null order");
            return;
        }

        if (_activeOrders.Contains(order))
        {
            order.Complete();
            _activeOrders.Remove(order);
            Debug.Log($"Order completed: {order.Recipe.RecipeName}");
        }
        else
        {
            Debug.LogWarning("Order not found in active orders");
        }
    }

    public List<Order> GetActiveOrders()
    {
        return new List<Order>(_activeOrders);
    }

    public List<RecipeData> GetActiveRecipes()
    {
        return _activeOrders.Select(order => order.Recipe).ToList();
    }

    private RecipeData GetRandomRecipe()
    {
        if (_activeOrders.Count == 0)
        {
            Debug.LogWarning("No active orders to select a recipe from");
            return null;
        }

        var randomIndex = Random.Range(0, _activeOrders.Count);
        return _activeOrders[randomIndex].Recipe;
    }
}
