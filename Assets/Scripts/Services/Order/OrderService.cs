using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderService : IOrderService
{
    private readonly List<Order> _activeOrders = new();
    private const float DEFAULT_TIME_LIMIT = 60f;

    public void AddOrder(RecipeData recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("Cannot add order with null recipe");
            return;
        }

        var order = new Order(recipe, DEFAULT_TIME_LIMIT);
        _activeOrders.Add(order);
        Debug.Log($"New order added: {recipe.RecipeName} (Time limit: {DEFAULT_TIME_LIMIT}s)!");
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
}
