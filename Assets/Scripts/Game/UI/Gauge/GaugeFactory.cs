using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;

public class GaugeFactory
{
    private readonly Camera _camera;
    private readonly RectTransform _canvasRectTransform;

    private readonly ObjectPool<GaugeView> _viewPool;
    private readonly GameObject _viewPrefab;
    private const int MaxPoolSize = 20;

    public GaugeFactory(RectTransform canvasRectTransform)
    {
        _canvasRectTransform = canvasRectTransform;
        _camera = Camera.main;

        // 어드레서블에서 GaugeView 프리팹 로드 (동기)
        var handle = Addressables.LoadAssetAsync<GameObject>(PrefabKeys.GetPrefabPath(PrefabKeys.KitchenGauge));
        _viewPrefab = handle.WaitForCompletion();

        // Unity ObjectPool 생성
        _viewPool = new ObjectPool<GaugeView>(
            createFunc: CreateView,
            actionOnGet: OnGetView,
            actionOnRelease: OnReleaseView,
            actionOnDestroy: OnDestroyView,
            defaultCapacity: 5,
            maxSize: MaxPoolSize
        );
    }

    private GaugeView CreateView()
    {
        var go = Object.Instantiate(_viewPrefab, _canvasRectTransform);
        return go.GetComponent<GaugeView>();
    }

    private void OnGetView(GaugeView view)
    {
        view.gameObject.SetActive(true);
    }

    private void OnReleaseView(GaugeView view)
    {
        view.gameObject.SetActive(false);
        view.SetIcon(null); // 아이콘 정리
    }

    private void OnDestroyView(GaugeView view)
    {
        if (view != null)
        {
            Object.Destroy(view.gameObject);
        }
    }

    public GaugePresenter Create(
        Transform target,
        Vector3 offset,
        Sprite icon = null)
    {
        // 1. View를 풀에서 가져오기
        GaugeView view = _viewPool.Get();

        // 2. 새 Presenter 생성 (Model 자동 생성됨)
        var presenter = new GaugePresenter(_camera, _canvasRectTransform, view, target, offset, icon);
        return presenter;
    }

    public void Release(GaugePresenter presenter)
    {
        if (presenter == null) return;

        var view = presenter.View;

        // 1. Presenter 정리 (Model, 구독 정리)
        presenter.Dispose();

        // 2. View를 풀에 반환
        if (view != null)
        {
            _viewPool.Release(view);
        }
    }
}
