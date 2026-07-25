// 상점 슬롯 1칸의 오퍼. 카드/계산가격/색/솔드아웃을 담는다.
public class CardOffer
{
    public CardData Card;      // SoldOut 이면 null
    public int Price;          // Cost + PriceIncrement * 보유수
    public CardColor Color;    // 카드에서 파생 (테이블=Yellow, 1단계=Green, 2단계+=Blue)
    public bool SoldOut;
}
