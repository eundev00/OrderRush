using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Order Rush/Card/Card", order = 20)]
public class CardData : ScriptableObject
{
    public int CardID;
    public string CardName;
    public string Description;
    public Sprite Icon;
    public int Cost;              // 베이스 가격
    public int PriceIncrement;    // 단계(보유수)마다 가격 증가폭. 실제가격 = Cost + PriceIncrement * 보유수
    public int MaxTier = 1;       // 최대 구매 단계(테이블 4, 버프 3, 메뉴/기타 1)
    public int Weight = 50; // 높을수록 더 자주 등장
    public bool IsOneDayOnly; // 원데이 전용(알바 등): 소유 아님, 하루만 쓰고 사라짐 → 구매기록/누적 대상 아님
    public CardEffectData Effect;
}
