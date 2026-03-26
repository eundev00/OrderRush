using System.Collections.Generic;

public interface IOrderService
{
    void AddOrder(RecipeData recipe);
    void CompleteOrder(Order order);
    List<Order> GetActiveOrders();
    List<RecipeData> GetActiveRecipes();
}
