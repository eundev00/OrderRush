using System;
using Cysharp.Threading.Tasks;
using UniRx;

public interface IPopupPresenter
{
    void RequestClose();
}

public abstract class PopupPresenterBase<TArgs, TResult> : IPopupPresenter, IDisposable
{
    private readonly PopupViewBase _view;
    private readonly UniTaskCompletionSource<TResult> _completion = new();
    private bool _closed;

    protected readonly CompositeDisposable Disposables = new();

    public PopupViewBase View => _view;

    protected PopupPresenterBase(PopupViewBase view)
    {
        _view = view;
    }

    public UniTask<TResult> ShowAsync(TArgs args)
    {
        OnBind();
        OnShow(args);
        _view.Show();
        return _completion.Task;
    }

    protected void Close(TResult result)
    {
        if (_closed)
            return;
        _closed = true;

        OnClose();
        _view.Hide();
        _completion.TrySetResult(result);
    }

    void IPopupPresenter.RequestClose()
    {
        Close(default);
    }

    protected virtual void OnBind() { }
    protected virtual void OnShow(TArgs args) { }
    protected virtual void OnClose() { }

    public void Dispose()
    {
        Disposables.Dispose();
        _completion.TrySetResult(default);
    }
}

public abstract class PopupPresenterBase<TArgs> : PopupPresenterBase<TArgs, bool>
{
    protected PopupPresenterBase(PopupViewBase view) : base(view) { }

    protected void Close() => Close(true);
}

public readonly struct NoArgs { }

public abstract class PopupPresenterBaseNoArgs<TResult> : PopupPresenterBase<NoArgs, TResult>
{
    protected PopupPresenterBaseNoArgs(PopupViewBase view) : base(view) { }

    protected sealed override void OnShow(NoArgs args) => OnShow();
    protected virtual void OnShow() { }

    public UniTask<TResult> ShowAsync() => ShowAsync(default);
}

public abstract class PopupPresenterBase : PopupPresenterBaseNoArgs<bool>
{
    protected PopupPresenterBase(PopupViewBase view) : base(view) { }

    protected void Close() => Close(true);
}
