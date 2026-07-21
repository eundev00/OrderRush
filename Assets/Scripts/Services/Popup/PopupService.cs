using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

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

        // 기존 자체 생성 Canvas가 있으면 파괴
        if (_host != null)
        {
            UnityEngine.Object.Destroy(_host);
            _host = null;
            _dim = null;
        }

        _canvasRoot = canvasRoot;

        if (_dim == null)
            CreateDim();
    }

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

        _catalog[popupKey] = parent;
    }

    public void UnregisterPopup(string popupKey)
    {
        _catalog.Remove(popupKey);
    }

    public void CloseOwnedBy(IObjectResolver parent)
    {
        if (parent == null)
            return;

        var owned = _stack.Where(e => e.Owner == parent).ToList();
        foreach (var entry in owned)
            entry.Presenter.RequestClose();
    }

    public UniTask<TResult> Open<TPresenter, TArgs, TResult>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs, TResult>
        => OpenInternal<TPresenter, TResult>(popupKey, p => p.ShowAsync(args));

    public async UniTask Open<TPresenter, TArgs>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs>
        => await OpenInternal<TPresenter, bool>(popupKey, p => p.ShowAsync(args));

    public UniTask<TResult> Open<TPresenter, TResult>(string popupKey)
        where TPresenter : PopupPresenterBaseNoArgs<TResult>
        => OpenInternal<TPresenter, TResult>(popupKey, p => p.ShowAsync());

    public async UniTask Open<TPresenter>(string popupKey)
        where TPresenter : PopupPresenterBase
        => await OpenInternal<TPresenter, bool>(popupKey, p => p.ShowAsync());

    private async UniTask<TResult> OpenInternal<TPresenter, TResult>(string popupKey, Func<TPresenter, UniTask<TResult>> show)
        where TPresenter : class, IPopupPresenter
    {
        if (!_initialized)
        {
            Debug.LogError("[PopupService] Open before Initialize.");
            return default;
        }

        if (_canvasRoot == null)
            CreateCanvas();

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

        _stack[_stack.Count - 1].Presenter.RequestClose();
    }

    public void CloseAll()
    {
        foreach (var entry in _stack.ToList())
            entry.Presenter.RequestClose();
    }

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
        if (!_stack.Remove(entry))
            return;

        entry.Scope.Dispose();
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
        img.raycastTarget = true;

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

    private sealed class PopupEntry
    {
        public readonly IPopupPresenter Presenter;
        public readonly IScopedObjectResolver Scope;
        public readonly GameObject Go;
        public readonly IObjectResolver Owner;

        public PopupEntry(IPopupPresenter presenter, IScopedObjectResolver scope, GameObject go, IObjectResolver owner)
        {
            Presenter = presenter;
            Scope = scope;
            Go = go;
            Owner = owner;
        }
    }
}
