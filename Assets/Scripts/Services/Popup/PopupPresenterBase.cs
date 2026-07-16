using System;
using Cysharp.Threading.Tasks;
using UniRx;

// =====================================================================
//  팝업 Presenter 공통 베이스
// ---------------------------------------------------------------------
//  - PopupService 가 자식 스코프에서 resolve 하며, 생성자로 자기 View(구체 타입)
//    + Project 스코프 서비스를 주입받는다. (Game 스코프 서비스는 주입 불가 →
//    필요한 데이터는 Args 로 받고, 게임 로직은 여는 쪽이 결과로 처리)
//  - ShowAsync(args) 는 "바인딩 → 표시 → 닫힘까지 대기" 를 캡슐화하고,
//    Close(result) 가 호출되면 결과와 함께 완료된다.
//  - 버튼 리스너/구독은 Disposables 에 담고, 스코프 Dispose 시 자동 해제.
//
//  파생 규약: OnBind()(리스너 등록) / OnShow(args)(데이터 반영) / OnClose() 오버라이드.
// =====================================================================

// PopupService 가 제네릭에 무관하게 스택에서 다루기 위한 비제네릭 계약.
public interface IPopupPresenter
{
    // 외부(PopupService)에서 강제 닫기 — 결과는 기본값
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

    // PopupService.Open 이 직접 호출: 바인딩 → 표시 → 닫힘까지 대기.
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

    // ---- 파생 훅 ----
    protected virtual void OnBind() { }
    protected virtual void OnShow(TArgs args) { }
    protected virtual void OnClose() { }

    public void Dispose()
    {
        Disposables.Dispose();
        _completion.TrySetResult(default); // 미완료 상태로 스코프가 정리될 때의 안전장치
    }
}

// 결과가 없는 팝업(예: 확인 버튼만 있는 메시지 팝업)용 단축 베이스.
// 내부적으로 bool 결과를 쓰되 호출부는 결과를 신경 쓰지 않는다.
public abstract class PopupPresenterBase<TArgs> : PopupPresenterBase<TArgs, bool>
{
    protected PopupPresenterBase(PopupViewBase view) : base(view) { }

    protected void Close() => Close(true);
}

// 입력 데이터가 없는 팝업용 빈 Args.
public readonly struct NoArgs { }

// Args 없음 + 결과 있음 (예: 문구 고정 확인/취소).
public abstract class PopupPresenterBaseNoArgs<TResult> : PopupPresenterBase<NoArgs, TResult>
{
    protected PopupPresenterBaseNoArgs(PopupViewBase view) : base(view) { }

    // args 버전을 봉인하고 인자 없는 훅으로 위임.
    protected sealed override void OnShow(NoArgs args) => OnShow();
    protected virtual void OnShow() { }

    public UniTask<TResult> ShowAsync() => ShowAsync(default);
}

// Args 없음 + 결과 없음 (예: 단순 안내창).
public abstract class PopupPresenterBase : PopupPresenterBaseNoArgs<bool>
{
    protected PopupPresenterBase(PopupViewBase view) : base(view) { }

    protected void Close() => Close(true);
}
