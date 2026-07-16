using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UniRx;

public class PopupCardShopPresenter : PopupPresenterBase
{
    private readonly PopupCardShop _view;
    private readonly IShopService _shopService;
    private readonly IAccountService _accountService;
    private readonly IDayProgressService _dayProgressService;

    public PopupCardShopPresenter(
        PopupCardShop view,
        IShopService shopService,
        IAccountService accountService,
        IDayProgressService dayProgressService) : base(view)
    {
        _view = view;
        _shopService = shopService;
        _accountService = accountService;
        _dayProgressService = dayProgressService;
    }

    protected override void OnBind()
    {
        _view.SkipButton.onClick.AddListener(OnSkipButtonClicked);
        _view.RefreshButton.onClick.AddListener(OnRefreshButtonClicked);

        Disposables.Add(Disposable.Create(() =>
        {
            _view.SkipButton.onClick.RemoveListener(OnSkipButtonClicked);
            _view.RefreshButton.onClick.RemoveListener(OnRefreshButtonClicked);
        }));
    }

    protected override void OnShow()
    {
        var randomCards = _shopService.GetRandomCardsForSelection(3);
        int currentCoins = _accountService.Account.Coins.Value;

        _view.SetupCards(randomCards, currentCoins, OnCardClicked);

        int refreshCost = _shopService.GetRefreshCost();
        _view.SetupRefreshButton(refreshCost, currentCoins >= refreshCost);
    }

    private async void OnCardClicked(CardData card)
    {
        bool success = await _shopService.TryPurchaseCard(card);
        if (success)
        {
            Close();
            _dayProgressService.NextDay();
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
        Close();
        _dayProgressService.NextDay();
    }
}
