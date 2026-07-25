using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupCardShop : PopupViewBase
{
    [NotNull][SerializeField] private CardItemView[] _cardItems;
    [NotNull][SerializeField] private Button _skipButton;

    public Button SkipButton => _skipButton;

    public void SetupOffer(List<CardOffer> offers, int currentCoins, Action<CardData> onCardClicked, List<Sprite> backgroundSprites)
    {
        for (int i = 0; i < _cardItems.Length; i++)
        {
            if (i < offers.Count)
            {
                var offer = offers[i];
                var sprite = i < backgroundSprites.Count ? backgroundSprites[i] : null;
                bool canPurchase = offer != null && !offer.SoldOut && offer.Card != null
                                   && currentCoins >= offer.Price;
                _cardItems[i].SetupOffer(offer, canPurchase, onCardClicked, sprite);
                _cardItems[i].gameObject.SetActive(true);
            }
            else
            {
                _cardItems[i].gameObject.SetActive(false);
            }
        }
    }
}
