using UniRx;

// 메시지 팝업 Presenter — 확인 버튼만 있는 안내 팝업.
// 결과 없는 베이스(PopupPresenterBase<MessageArgs>) 사용: 확인 → Close().
public class MessagePopupPresenter : PopupPresenterBase<MessageArgs>
{
    private readonly MessagePopupView _view;

    public MessagePopupPresenter(MessagePopupView view) : base(view)
    {
        _view = view;
    }

    protected override void OnBind()
    {
        _view.ConfirmButton.onClick.AddListener(Close);
        Disposables.Add(Disposable.Create(
            () => _view.ConfirmButton.onClick.RemoveListener(Close)));
    }

    protected override void OnShow(MessageArgs args)
    {
        _view.SetMessage(args.Message);
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
