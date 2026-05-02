using System.Collections.Generic;

public interface IOrderService
{
    Order AddOrder();
    void CompleteOrder(Order order);
    List<Order> GetActiveOrders();
    List<RecipeData> GetActiveRecipes();
}
