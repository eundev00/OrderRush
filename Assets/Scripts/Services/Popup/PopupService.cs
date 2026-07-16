using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

// =====================================================================
//  전역 팝업 서비스 구현 — 동적 로드 + 자식 스코프 주입 + 딤/모달 + 스택.
// ---------------------------------------------------------------------
//  흐름(Open):
//   1) Presenter 타입의 [PopupPrefabKey] 로 프리팹 키 조회
//   2) IResourcesLoaderService 로 프리팹 로드 → 팝업 캔버스 아래 Instantiate
//   3) 자식 스코프 생성(RegisterInstance(view) + Register<TPresenter>) → Presenter resolve
//      (View + Project 스코프 서비스 주입. Game 스코프 서비스는 주입 불가 → Args 로)
//   4) 스택 push + 딤 갱신 → await presenter.ShowAsync(args)  (Close 호출 시 완료)
//   5) finally: 스택 pop → 자식 스코프 Dispose(Presenter Dispose) → 팝업 GO Destroy → 딤 갱신
//
//  캔버스는 DontDestroyOnLoad 로 코드 생성(SoundService 호스트 방식) → 씬 의존 없음.
//  ⚠ 씬에 EventSystem 이 있어야 버튼 입력이 동작한다(기존 HUD 등이 이미 사용 중).
// =====================================================================
public class PopupService : IPopupService, IDisposable
{
    private const int PopupSortingOrder = 1000; // HUD 등 위에 표시
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);

    private readonly IObjectResolver _resolver;
    private readonly IResourcesLoaderService _loader;

    private readonly List<PopupEntry> _stack = new();

    private RectTransform _canvasRoot;
    private GameObject _host;
    private GameObject _dim;
    private bool _initialized;

    public bool HasOpenPopup => _stack.Count > 0;

    public PopupService(IObjectResolver resolver, IResourcesLoaderService loader)
    {
        _resolver = resolver;
        _loader = loader;
    }

    public UniTask Initialize()
    {
        if (_initialized)
            return UniTask.CompletedTask;

        CreateCanvas();
        _initialized = true;
        return UniTask.CompletedTask;
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
        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            _stack[i].Presenter.RequestClose();
        }
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

        // 자식 스코프: 이 팝업 인스턴스(View)와 Presenter 만 등록.
        var scope = _resolver.CreateScope(builder =>
        {
            builder.RegisterInstance(view).As(view.GetType());
            builder.Register<TPresenter>(Lifetime.Scoped);
        });

        var presenter = scope.Resolve<TPresenter>();

        var entry = new PopupEntry(presenter, scope, go);
        _stack.Add(entry);
        return entry;
    }

    private void CloseEntry(PopupEntry entry)
    {
        if (!_stack.Remove(entry))
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

        public PopupEntry(IPopupPresenter presenter, IScopedObjectResolver scope, GameObject go)
        {
            Presenter = presenter;
            Scope = scope;
            Go = go;
        }
    }
}
