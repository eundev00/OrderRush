using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCardShop : PopupViewBase
{
    [NotNull][SerializeField] private CardItemView[] _cardItems;
    [NotNull][SerializeField] private Button _skipButton;
    [NotNull][SerializeField] private Button _refreshButton;
    [NotNull][SerializeField] private TMP_Text _refreshCostText;
    [NotNull][SerializeField] private TMP_Text _coinsText;

    public Button SkipButton => _skipButton;
    public Button RefreshButton => _refreshButton;

    public void SetupCards(List<CardData> cards, int currentCoins, Action<CardData> onCardClicked)
    {
        _coinsText.text = $"{currentCoins}";
        for (int i = 0; i < _cardItems.Length; i++)
        {
            if (i < cards.Count)
            {
                bool canPurchase = currentCoins >= cards[i].Cost;
                _cardItems[i].Setup(cards[i], canPurchase, onCardClicked);
                _cardItems[i].gameObject.SetActive(true);
            }
            else
            {
                _cardItems[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetupRefreshButton(int cost, bool canAfford)
    {
        _refreshCostText.text = $"{cost}";
        _refreshButton.interactable = canAfford;
    }
}
