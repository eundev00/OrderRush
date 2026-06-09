using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace OrderRush.Services
{
    public interface IShopService
    {
        void Initialize();
        List<CardData> GetRandomCardsForSelection(int count);
        UniTask<bool> TryPurchaseCard(CardData card);
        int GetRefreshCost();
        UniTask<List<CardData>> Refresh();
    }
}
