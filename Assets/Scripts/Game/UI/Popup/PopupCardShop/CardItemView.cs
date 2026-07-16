using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardItemView : MonoBehaviour
{
    [NotNull][SerializeField] private Image _iconImage;
    [NotNull][SerializeField] private TMP_Text _nameText;
    [NotNull][SerializeField] private TMP_Text _descriptionText;
    [NotNull][SerializeField] private TMP_Text _costText;
    [NotNull][SerializeField] private GameObject _descView;
    [NotNull][SerializeField] private Button _buyButton;
    [NotNull][SerializeField] private Button _moreButton;
    [NotNull][SerializeField] private Button _moreCloseButton;

    private CardData _cardData;
    private Action<CardData> _onCardClicked;


    private void Awake()
    {
        _descView.SetActive(false);
    }


    private void OnEnable()
    {
        _buyButton.onClick.AddListener(OnBuyButtonClicked);
        _moreButton.onClick.AddListener(OnMoreButtonClicked);
        _moreCloseButton.onClick.AddListener(OnMoreCloseButtonClicked);
    }

    private void OnDisable()
    {
        _buyButton.onClick.RemoveAllListeners();
        _moreButton.onClick.RemoveAllListeners();
        _moreCloseButton.onClick.RemoveAllListeners();
    }

    public void Setup(CardData cardData, bool canPurchase, Action<CardData> onCardClicked)
    {
        _cardData = cardData;
        _onCardClicked = onCardClicked;

        _nameText.text = cardData.CardName;
        _descriptionText.text = cardData.Description;
        _costText.text = cardData.Cost.ToString();

        _buyButton.interactable = canPurchase;
    }

    private void OnBuyButtonClicked()
    {
        _onCardClicked?.Invoke(_cardData);
    }

    private void OnMoreButtonClicked()
    {
        _descView.SetActive(true);
    }

    private void OnMoreCloseButtonClicked()
    {
        _descView.SetActive(false);
    }
}
