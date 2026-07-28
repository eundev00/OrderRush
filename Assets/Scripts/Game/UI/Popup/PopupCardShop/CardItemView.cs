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
    [NotNull][SerializeField] private TMP_Text _costTextDisabled;
    [NotNull][SerializeField] private GameObject _descView;
    [NotNull][SerializeField] private Button _buyButton;
    [NotNull][SerializeField] private Button _moreButton;
    [NotNull][SerializeField] private Button _moreCloseButton;
    [NotNull][SerializeField] private Image _colorFrame;          // 색 프레임 (없으면 색 표시 스킵)
    [NotNull][SerializeField] private GameObject _soldOutOverlay;  // SOLD OUT 오버레이

    private CardData _cardData;
    private Action<CardData> _onCardClicked;


    private void Awake()
    {
        _descView.SetActive(false);
        _soldOutOverlay.SetActive(false);
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

        if (_iconImage != null && cardData.Icon != null)
        {
            _iconImage.sprite = cardData.Icon;
        }

        if (canPurchase)
        {
            _costText.text = cardData.Cost.ToString();
            _costText.gameObject.SetActive(true);
            _costTextDisabled.gameObject.SetActive(false);
        }
        else
        {
            _costTextDisabled.text = cardData.Cost.ToString();
            _costTextDisabled.gameObject.SetActive(true);
            _costText.gameObject.SetActive(false);
        }

        _buyButton.interactable = canPurchase;
    }

    public void SetupOffer(CardOffer offer, bool canPurchase, Action<CardData> onCardClicked, Sprite backgroundSprite)
    {
        bool soldOut = offer == null || offer.SoldOut || offer.Card == null;
        if (_soldOutOverlay != null)
            _soldOutOverlay.SetActive(soldOut);

        if (soldOut)
        {
            _cardData = null;
            _onCardClicked = null;
            _buyButton.interactable = false;
            return;
        }

        _cardData = offer.Card;
        _onCardClicked = onCardClicked;

        _nameText.text = offer.Card.CardName;
        _descriptionText.text = offer.Card.Description;

        if (_iconImage != null && offer.Card.Icon != null)
        {
            _iconImage.sprite = offer.Card.Icon;
        }

        SetBackground(backgroundSprite);
        SetPrice(offer.Price, canPurchase);

        _buyButton.interactable = canPurchase;
    }

    private void SetPrice(int price, bool canPurchase)
    {
        if (canPurchase)
        {
            _costText.text = price.ToString();
            _costText.gameObject.SetActive(true);
            _costTextDisabled.gameObject.SetActive(false);
        }
        else
        {
            _costTextDisabled.text = price.ToString();
            _costTextDisabled.gameObject.SetActive(true);
            _costText.gameObject.SetActive(false);
        }
    }

    private void SetBackground(Sprite sprite)
    {
        if (_colorFrame == null) return;
        _colorFrame.sprite = sprite;
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
