using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardsData", menuName = "Order Rush/Card/Cards Data", order = 21)]
public class CardsData : ScriptableObject
{
    public List<CardData> Cards;
}
