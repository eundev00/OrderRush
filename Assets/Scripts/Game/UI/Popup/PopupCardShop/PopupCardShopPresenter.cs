using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.U2D;

public class PopupCardShopPresenter : PopupPresenterBase
{
    private readonly PopupCardShop _view;
    private readonly IShopService _shopService;
    private readonly IAccountService _accountService;
    private readonly IDayProgressService _dayProgressService;
    private readonly ISoundService _soundService;
    private readonly IResourcesLoaderService _resourcesLoader;

    public PopupCardShopPresenter(
        PopupCardShop view,
        IShopService shopService,
        IAccountService accountService,
        IDayProgressService dayProgressService,
        ISoundService soundService,
        IResourcesLoaderService resourcesLoader) : base(view)
    {
        _view = view;
        _shopService = shopService;
        _accountService = accountService;
        _dayProgressService = dayProgressService;
        _soundService = soundService;
        _resourcesLoader = resourcesLoader;
    }

    protected override void OnBind()
    {
        _view.SkipButton.onClick.AddListener(OnSkipButtonClicked);

        Disposables.Add(Disposable.Create(() =>
        {
            _view.SkipButton.onClick.RemoveListener(OnSkipButtonClicked);
        }));
    }

    protected override async void OnShow()
    {
        var atlas = await _resourcesLoader.LoadAsync<SpriteAtlas>(AtlasKeys.GetAtlasPath(AtlasKeys.UIAtlas));
        var offers = _shopService.GetCurrentOffer();
        int currentCoins = _accountService.Account.Coins.Value;

        var sprites = new List<Sprite>();
        foreach (var offer in offers)
        {
            if (offer != null && !offer.SoldOut && offer.Card != null)
            {
                var spriteName = GetSpriteNameFromColor(offer.Color);
                sprites.Add(atlas.GetSprite(spriteName));
            }
            else
            {
                sprites.Add(null);
            }
        }

        _view.SetupOffer(offers, currentCoins, OnCardClicked, sprites);
    }

    private async void OnCardClicked(CardData card)
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        bool success = await _shopService.TryPurchaseCard(card);
        if (success)
        {
            Close();
            _dayProgressService.NextDay();
        }
    }

    private void OnSkipButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close();
        _dayProgressService.NextDay();
    }

    private string GetSpriteNameFromColor(CardColor color)
    {
        return color switch
        {
            CardColor.Yellow => "CardYellow",
            CardColor.Green => "CardGreen",
            CardColor.Blue => "CardBlue",
            _ => "CardYellow"
        };
    }
}
