using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace OrderRush.Services
{
    public interface ICardService
    {
        void Initialize();
        List<CardData> GetRandomCardsForSelection(int count);
        UniTask<bool> TryPurchaseCard(CardData card);
    }
}
