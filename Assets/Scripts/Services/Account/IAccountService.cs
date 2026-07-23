using System.Collections.Generic;
using OrderRush.Data;
using OrderRush.Models;

public interface IAccountService
{
    Account Account { get; }

    void Initialize();
    void AddCoins(int amount);
    void SpendCoins(int amount);
    bool TrySpendCoins(int amount);
    void AddOwnedRecipe(int recipeID);
    int GetRandomOwnedRecipeID();
    void SetCurrentProgress(int day);
    void AddPurchasedCard(int cardID);
    List<int> GetPurchasedCardIDs();
    void ResetAll();
}
