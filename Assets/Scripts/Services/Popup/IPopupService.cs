using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

// =====================================================================
//  전역 팝업 서비스 — 키로 팝업을 열고 결과를 await 로 받는다.
// ---------------------------------------------------------------------
//  - Project 스코프 Singleton. DontDestroyOnLoad 팝업 캔버스를 소유하며
//    로비/게임 어디서든 동일 API 로 팝업을 띄운다.
//  - "부모 리졸버 카탈로그" 방식: 팝업 키마다 "그 팝업을 등록한 쪽의 리졸버"를
//    기억해 두고(RegisterPopup), Open 시 그 리졸버의 자식 스코프에서 Presenter 를
//    resolve 한다. → 게임에서 등록한 팝업은 게임 서비스를, 공통(Root) 팝업은
//    Root 서비스를 자동으로 주입받는다. 호출자는 scope 를 넘길 필요가 없다.
//  - 프리팹은 키(PrefabKeys)로 동적 로드→Instantiate 한다.
// =====================================================================
public interface IPopupService
{
    UniTask Initialize();
    void SetCanvasRoot(RectTransform canvasRoot);

    // ---- 카탈로그 ----
    // 팝업 키를 "그 팝업 Presenter 가 태어날 부모 스코프" 와 함께 등록.
    // 공통 팝업은 Root 리졸버로, 씬 전용 팝업은 그 씬 리졸버로 등록한다.
    void RegisterPopup(string popupKey, IObjectResolver parent);
    void UnregisterPopup(string popupKey);

    // 특정 부모(씬)로 열린 팝업을 일괄 정리 — 씬 종료 시 호출.
    void CloseOwnedBy(IObjectResolver parent);

    // ---- 열기 ----
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
