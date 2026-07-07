using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Order Rush/Card/Card", order = 20)]
public class CardData : ScriptableObject
{
    public int CardID;
    public string CardName;
    public string Description;
    public Sprite Icon;
    public int Cost;
    public int Weight = 50; // 높을수록 더 자주 등장
    public bool IsExpiring;
    public CardEffectData Effect;
}
