using System;
using VContainer;
using VContainer.Unity;

// =====================================================================
//  씬 전용 팝업 등록기 — 씬 스코프에 엔트리포인트로 등록해서 사용.
// ---------------------------------------------------------------------
//  - 생성자로 "그 씬의 리졸버(IObjectResolver)" + IPopupService + 등록할 키 목록을 받는다.
//  - Start(): 키들을 이 씬 리졸버로 RegisterPopup → 해당 팝업은 이 씬의 자식 스코프에서
//    생성되어 씬 서비스를 주입받는다.
//  - Dispose(): 이 씬으로 열린 팝업을 모두 닫고(CloseOwnedBy) 카탈로그에서 해제 → 고아 방지.
//
//  ⚠ LifetimeScope.Configure() 는 빌드 시점이라 리졸버가 미완성 → 반드시 이 엔트리포인트의
//    Start() 에서 등록해야 한다(주입된 IObjectResolver 는 이 씬 스코프의 리졸버).
//
//  등록 예 (씬 LifetimeScope.Configure) — 타입 기반 주입(문자열 매칭 리스크 없음):
//    builder.RegisterInstance(new ScenePopupKeys(PrefabKeys.PopupCompleted, ...));
//    builder.RegisterEntryPoint<ScenePopupRegistrar>();
// =====================================================================
public class ScenePopupRegistrar : IStartable, IDisposable
{
    private readonly IObjectResolver _resolver;
    private readonly IPopupService _popupService;
    private readonly ScenePopupKeys _popupKeys;

    public ScenePopupRegistrar(
        IObjectResolver resolver,
        IPopupService popupService,
        ScenePopupKeys popupKeys)
    {
        _resolver = resolver;
        _popupService = popupService;
        _popupKeys = popupKeys;
    }

    public void Start()
    {
        foreach (var key in _popupKeys.Keys)
            _popupService.RegisterPopup(key, _resolver);
    }

    public void Dispose()
    {
        // 이 씬으로 열린 팝업을 먼저 정리한 뒤 카탈로그에서 해제.
        _popupService.CloseOwnedBy(_resolver);
        foreach (var key in _popupKeys.Keys)
            _popupService.UnregisterPopup(key);
    }
}
