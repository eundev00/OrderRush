using UnityEngine;

public class KitchenGaugeFactory : BaseUIFactory<GaugeView, KitchenGaugePresenter>
{
    public KitchenGaugeFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.KitchenGauge)
    {
    }

    protected override KitchenGaugePresenter CreatePresenter(Transform target, Vector3 offset)
    {
        GaugeView view = GetViewFromPool();
        var presenter = new KitchenGaugePresenter(_camera, _canvasRectTransform, view, target, offset);
        return presenter;
    }

    protected override void OnReleaseView(GaugeView view)
    {
        base.OnReleaseView(view);
    }
}
