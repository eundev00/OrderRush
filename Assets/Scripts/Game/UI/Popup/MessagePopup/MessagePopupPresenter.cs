using UniRx;

// 메시지 팝업 Presenter — 확인 버튼만 있는 안내 팝업.
// 결과 없는 베이스(PopupPresenterBase<MessageArgs>) 사용: 확인 → Close().
public class MessagePopupPresenter : PopupPresenterBase<MessageArgs>
{
    private readonly MessagePopupView _view;
    private readonly ISoundService _soundService;

    public MessagePopupPresenter(MessagePopupView view, ISoundService soundService) : base(view)
    {
        _view = view;
        _soundService = soundService;
    }

    protected override void OnBind()
    {
        _view.ConfirmButton.onClick.AddListener(OnConfirmButtonClicked);
        Disposables.Add(Disposable.Create(
            () => _view.ConfirmButton.onClick.RemoveListener(OnConfirmButtonClicked)));
    }

    protected override void OnShow(MessageArgs args)
    {
        _view.SetMessage(args.Message);
    }

    private void OnConfirmButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close();
    }
}

// 메시지 팝업 입력값. 필요 시 confirmText 등 확장.
public readonly struct MessageArgs
{
    public readonly string Message;

    public MessageArgs(string message)
    {
        Message = message;
    }
}
