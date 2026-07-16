using Cysharp.Threading.Tasks;

// =====================================================================
//  전역 팝업 서비스 — 키로 팝업을 열고 결과를 await 로 받는다.
// ---------------------------------------------------------------------
//  - Project 스코프 Singleton. DontDestroyOnLoad 팝업 캔버스를 소유하며
//    로비/게임 어디서든 동일 API 로 팝업을 띄운다.
//  - Presenter 타입의 [PopupPrefabKey] 로 프리팹을 동적 로드→Instantiate,
//    자식 스코프에서 resolve 해 View/Project서비스를 주입한다.
// =====================================================================
public interface IPopupService
{
    UniTask Initialize();

    // Args O, 결과 O (예: 확인/취소 → bool, 선택 결과 등)
    UniTask<TResult> Open<TPresenter, TArgs, TResult>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs, TResult>;

    // Args O, 결과 X (예: 확인 버튼만 있는 메시지 팝업)
    UniTask Open<TPresenter, TArgs>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs>;

    // Args X, 결과 O (예: 문구 고정 확인/취소)
    UniTask<TResult> Open<TPresenter, TResult>(string popupKey)
        where TPresenter : PopupPresenterBaseNoArgs<TResult>;

    // Args X, 결과 X (예: 단순 안내창)
    UniTask Open<TPresenter>(string popupKey)
        where TPresenter : PopupPresenterBase;

    void CloseTop();
    void CloseAll();

    bool HasOpenPopup { get; }
}
