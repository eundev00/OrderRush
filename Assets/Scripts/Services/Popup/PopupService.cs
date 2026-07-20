using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

// =====================================================================
//  전역 팝업 서비스 구현 — 부모 리졸버 카탈로그 + 동적 로드 + 딤/모달 + 스택.
// ---------------------------------------------------------------------
//  흐름(Open):
//   1) 카탈로그에서 팝업 키의 "부모 리졸버" 조회 (RegisterPopup 으로 등록해 둔 것)
//   2) 키(PrefabKeys)로 프리팹 로드 → 팝업 캔버스 아래 Instantiate
//   3) 부모 리졸버의 자식 스코프 생성(RegisterInstance(view)+Register<TPresenter>) → resolve
//      → 팝업 Presenter 가 그 부모(+상위) 스코프 서비스까지 주입받는다.
//   4) 스택 push(Owner=부모 기록) + 딤 갱신 → await presenter.ShowAsync(args)
//   5) finally: 스택 pop → 자식 스코프 Dispose(Presenter Dispose) → 팝업 GO Destroy → 딤 갱신
//
//  ▷ 스코프: 호출자가 scope 를 넘기지 않는다. "누가 이 팝업을 등록했는가"(부모 리졸버)를
//    카탈로그가 기억하므로, 게임에서 등록한 팝업은 게임 스코프의 자식으로,
//    공통 팝업은 Root 의 자식으로 생성된다. 팝업이 다른 팝업을 열어도 부모는 항상
//    "등록 스코프"라 서로 형제로 독립 생성된다(체이닝 안전).
//
//  캔버스는 DontDestroyOnLoad 로 코드 생성(SoundService 호스트 방식) → 씬 의존 없음.
//  ⚠ 씬에 EventSystem 이 있어야 버튼 입력이 동작한다(기존 HUD 등이 이미 사용 중).
// =====================================================================
public class PopupService : IPopupService, IDisposable
{
    private const int PopupSortingOrder = 1000; // HUD 등 위에 표시
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);

    private readonly IResourcesLoaderService _loader;

    // 키 → 그 팝업 Presenter 가 태어날 부모 스코프.
    private readonly Dictionary<string, IObjectResolver> _catalog = new();
    private readonly List<PopupEntry> _stack = new();

    private RectTransform _canvasRoot;
    private GameObject _host;
    private GameObject _dim;
    private bool _initialized;

    public bool HasOpenPopup => _stack.Count > 0;

    public PopupService(IResourcesLoaderService loader)
    {
        _loader = loader;
    }

    public UniTask Initialize()
    {
        if (_initialized)
            return UniTask.CompletedTask;

        if (_canvasRoot == null)
            CreateCanvas();

        _initialized = true;
        return UniTask.CompletedTask;
    }

    public void SetCanvasRoot(RectTransform canvasRoot)
    {
        if (canvasRoot == null)
        {
            Debug.LogError("[PopupService] SetCanvasRoot called with null.");
            return;
        }

        _canvasRoot = canvasRoot;

        if (_dim == null)
            CreateDim();
    }

    // ---------------------------------------------------------------
    //  카탈로그
    // ---------------------------------------------------------------
    public void RegisterPopup(string popupKey, IObjectResolver parent)
    {
        if (string.IsNullOrEmpty(popupKey))
        {
            Debug.LogError("[PopupService] RegisterPopup with empty key.");
            return;
        }
        if (parent == null)
        {
            Debug.LogError($"[PopupService] RegisterPopup with null parent: {popupKey}");
            return;
        }

        _catalog[popupKey] = parent; // 같은 키 재등록 시 최신 부모로 갱신(씬 재진입 대비)
    }

    public void UnregisterPopup(string popupKey)
    {
        _catalog.Remove(popupKey);
    }

    public void CloseOwnedBy(IObjectResolver parent)
    {
        if (parent == null)
            return;

        // 스냅샷으로 순회 — RequestClose 가 각 Open 의 finally 를 태워 CloseEntry 로 정리한다.
        var owned = _stack.Where(e => e.Owner == parent).ToList();
        foreach (var entry in owned)
            entry.Presenter.RequestClose();
    }

    // ---------------------------------------------------------------
    //  Open
    // ---------------------------------------------------------------
    // Args O, 결과 O
    public UniTask<TResult> Open<TPresenter, TArgs, TResult>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs, TResult>
        => OpenInternal<TPresenter, TResult>(popupKey, p => p.ShowAsync(args));

    // Args O, 결과 X
    public async UniTask Open<TPresenter, TArgs>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs>
        => await OpenInternal<TPresenter, bool>(popupKey, p => p.ShowAsync(args));

    // Args X, 결과 O
    public UniTask<TResult> Open<TPresenter, TResult>(string popupKey)
        where TPresenter : PopupPresenterBaseNoArgs<TResult>
        => OpenInternal<TPresenter, TResult>(popupKey, p => p.ShowAsync());

    // Args X, 결과 X
    public async UniTask Open<TPresenter>(string popupKey)
        where TPresenter : PopupPresenterBase
        => await OpenInternal<TPresenter, bool>(popupKey, p => p.ShowAsync());

    // 공통 열기 루틴: 생성 → 딤 갱신 → 표시/대기 → 정리.
    private async UniTask<TResult> OpenInternal<TPresenter, TResult>(string popupKey, Func<TPresenter, UniTask<TResult>> show)
        where TPresenter : class, IPopupPresenter
    {
        if (!_initialized)
        {
            Debug.LogError("[PopupService] Open before Initialize.");
            return default;
        }

        var entry = await CreateEntry<TPresenter>(popupKey);
        if (entry == null)
            return default;

        UpdateDim();
        try
        {
            return await show((TPresenter)entry.Presenter);
        }
        finally
        {
            CloseEntry(entry);
            UpdateDim();
        }
    }

    public void CloseTop()
    {
        if (_stack.Count == 0)
            return;

        // 완료만 트리거 — 실제 정리는 해당 Open 의 finally 에서 수행.
        _stack[_stack.Count - 1].Presenter.RequestClose();
    }

    public void CloseAll()
    {
        // 위에서부터 닫기 (스냅샷으로 순회 — 정리는 각 Open finally 담당)
        foreach (var entry in _stack.ToList())
            entry.Presenter.RequestClose();
    }

    // ---------------------------------------------------------------
    //  내부
    // ---------------------------------------------------------------
    private async UniTask<PopupEntry> CreateEntry<TPresenter>(string popupKey)
        where TPresenter : class, IPopupPresenter
    {
        if (string.IsNullOrEmpty(popupKey))
        {
            Debug.LogError("[PopupService] Open called with empty popup key.");
            return null;
        }

        if (!_catalog.TryGetValue(popupKey, out var parent) || parent == null)
        {
            Debug.LogError($"[PopupService] Popup not registered (call RegisterPopup first): {popupKey}");
            return null;
        }

        var prefab = await _loader.LoadAsync<GameObject>(PrefabKeys.GetPrefabPath(popupKey));
        if (prefab == null)
        {
            Debug.LogError($"[PopupService] Popup prefab load failed: {popupKey}");
            return null;
        }

        var go = UnityEngine.Object.Instantiate(prefab, _canvasRoot);
        var view = go.GetComponent<PopupViewBase>();
        if (view == null)
        {
            Debug.LogError($"[PopupService] Prefab has no PopupViewBase: {popupKey}");
            UnityEngine.Object.Destroy(go);
            return null;
        }

        // 자식 스코프: 등록 당시 부모(parent)의 자식으로 만들어 이 팝업 인스턴스(View)와 Presenter 만 등록.
        // → Presenter 는 parent(+상위) 서비스 + 자기 View 를 주입받는다.
        var childScope = parent.CreateScope(builder =>
        {
            builder.RegisterInstance(view).As(view.GetType());
            builder.Register<TPresenter>(Lifetime.Scoped);
        });

        var presenter = childScope.Resolve<TPresenter>();

        var entry = new PopupEntry(presenter, childScope, go, parent);
        _stack.Add(entry);
        return entry;
    }

    private void CloseEntry(PopupEntry entry)
    {
        if (!_stack.Remove(entry)) // 이미 정리됨 → 이중 Dispose 방지
            return;

        entry.Scope.Dispose();       // Presenter.Dispose() → 리스너/구독 해제
        if (entry.Go != null)
            UnityEngine.Object.Destroy(entry.Go);
    }

    private void UpdateDim()
    {
        if (_dim == null)
            return;

        if (_stack.Count == 0)
        {
            _dim.SetActive(false);
            return;
        }

        // 딤을 최상단 팝업 바로 뒤에 배치(모달: 아래 콘텐츠 전부 차단).
        _dim.SetActive(true);
        _dim.transform.SetAsLastSibling();
        _stack[_stack.Count - 1].Go.transform.SetAsLastSibling();
    }

    private void CreateCanvas()
    {
        _host = new GameObject("[PopupCanvas]");
        UnityEngine.Object.DontDestroyOnLoad(_host);

        var canvas = _host.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = PopupSortingOrder;

        // TODO: referenceResolution 은 프로젝트 UI 캔버스 설정에 맞춰 조정.
        var scaler = _host.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _host.AddComponent<GraphicRaycaster>();
        _canvasRoot = (RectTransform)_host.transform;

        CreateDim();
    }

    private void CreateDim()
    {
        _dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)_dim.transform;
        rect.SetParent(_canvasRoot, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var img = _dim.GetComponent<Image>();
        img.color = DimColor;
        img.raycastTarget = true; // 뒤쪽 입력 차단

        _dim.SetActive(false);
    }

    public void Dispose()
    {
        CloseAll();
        _stack.Clear();
        _catalog.Clear();

        if (_host != null)
        {
            UnityEngine.Object.Destroy(_host);
            _host = null;
        }
    }

    // 열린 팝업 하나의 수명 묶음.
    private sealed class PopupEntry
    {
        public readonly IPopupPresenter Presenter;
        public readonly IScopedObjectResolver Scope;
        public readonly GameObject Go;
        public readonly IObjectResolver Owner; // 이 팝업을 만든 부모 스코프(씬 정리 기준)

        public PopupEntry(IPopupPresenter presenter, IScopedObjectResolver scope, GameObject go, IObjectResolver owner)
        {
            Presenter = presenter;
            Scope = scope;
            Go = go;
            Owner = owner;
        }
    }
}
