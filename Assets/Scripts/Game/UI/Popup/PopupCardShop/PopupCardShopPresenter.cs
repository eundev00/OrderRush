using System;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using VContainer.Unity;

public class PopupCardShopPresenter : IStartable, IDisposable
{
    private readonly PopupCardShop _view;
    private readonly IShopService _shopService;
    private readonly IAccountService _accountService;

    public PopupCardShopPresenter(
        PopupCardShop view,
        IShopService shopService,
        IAccountService accountService)
    {
        _view = view;
        _shopService = shopService;
        _accountService = accountService;
    }

    public void Start()
    {
        _view.Hide();
        _view.SkipButton.onClick.AddListener(OnSkipButtonClicked);
        _view.RefreshButton.onClick.AddListener(OnRefreshButtonClicked);
    }

    public void ShowPopup()
    {
        var randomCards = _shopService.GetRandomCardsForSelection(3);
        int currentCoins = _accountService.Account.Coins.Value;

        _view.SetupCards(randomCards, currentCoins, OnCardClicked);

        int refreshCost = _shopService.GetRefreshCost();
        _view.SetupRefreshButton(refreshCost, currentCoins >= refreshCost);

        _view.Show();
    }

    private async void OnCardClicked(CardData card)
    {
        bool success = await _shopService.TryPurchaseCard(card);
        if (success)
        {
            _view.Hide();
        }
    }

    private async void OnRefreshButtonClicked()
    {
        var newCards = await _shopService.Refresh();
        if (newCards == null) return;

        int currentCoins = _accountService.Account.Coins.Value;
        _view.SetupCards(newCards, currentCoins, OnCardClicked);

        int refreshCost = _shopService.GetRefreshCost();
        _view.SetupRefreshButton(refreshCost, currentCoins >= refreshCost);
    }

    private void OnSkipButtonClicked()
    {
        _view.Hide();
    }

    public void Dispose()
    {
        _view.SkipButton.onClick.RemoveListener(OnSkipButtonClicked);
        _view.RefreshButton.onClick.RemoveListener(OnRefreshButtonClicked);
    }
}
