using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CardsData", menuName = "Order Rush/Cards Data")]
public class CardsData : ScriptableObject
{
    public List<CardData> Cards;

    public CardData GetCard(int cardID)
    {
        return Cards.Find(c => c.CardID == cardID);
    }

    public List<CardData> GetRandomCards(int count, List<int> excludeIDs)
    {
        var availableCards = Cards.Where(c => !excludeIDs.Contains(c.CardID)).ToList();
        return availableCards.OrderBy(x => Random.value).Take(count).ToList();
    }
}
