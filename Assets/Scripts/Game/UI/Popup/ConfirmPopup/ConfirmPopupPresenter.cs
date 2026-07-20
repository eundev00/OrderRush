using UniRx;

public class ConfirmPopupPresenter : PopupPresenterBase<ConfirmPopupArgs, bool>
{
    private readonly ConfirmPopupView _view;

    public ConfirmPopupPresenter(ConfirmPopupView view) : base(view)
    {
        _view = view;
    }

    protected override void OnBind()
    {
        _view.ConfirmButton.onClick.AddListener(OnConfirmClicked);
        _view.CancelButton.onClick.AddListener(OnCancelClicked);

        Disposables.Add(Disposable.Create(() =>
        {
            _view.ConfirmButton.onClick.RemoveListener(OnConfirmClicked);
            _view.CancelButton.onClick.RemoveListener(OnCancelClicked);
        }));
    }

    protected override void OnShow(ConfirmPopupArgs args)
    {
        _view.SetMessage(args.Message);
        _view.SetButtonLabels(args.ConfirmLabel, args.CancelLabel);
    }

    private void OnConfirmClicked()
    {
        Close(true);
    }

    private void OnCancelClicked()
    {
        Close(false);
    }
}

public readonly struct ConfirmPopupArgs
{
    public readonly string Message;
    public readonly string ConfirmLabel;
    public readonly string CancelLabel;

    public ConfirmPopupArgs(string message, string confirmLabel = "확인", string cancelLabel = "취소")
    {
        Message = message;
        ConfirmLabel = confirmLabel;
        CancelLabel = cancelLabel;
    }
}
