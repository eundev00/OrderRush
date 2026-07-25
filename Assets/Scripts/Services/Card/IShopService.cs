using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IShopService
{
    UniTask<bool> TryPurchaseCard(CardData card);
    List<CardOffer> GetCurrentOffer();
}
