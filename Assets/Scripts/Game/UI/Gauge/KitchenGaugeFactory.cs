using UnityEngine;

public class KitchenGaugeFactory : BaseUIFactory<GaugeView, KitchenGaugePresenter>
{
    public KitchenGaugeFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.KitchenGauge)
    {
    }

    public override KitchenGaugePresenter Create(Transform target, Vector3 offset, Sprite icon = null)
    {
        GaugeView view = GetViewFromPool();
        var presenter = new KitchenGaugePresenter(_camera, _canvasRectTransform, view, target, offset, icon);
        return presenter;
    }

    protected override void OnReleaseView(GaugeView view)
    {
        base.OnReleaseView(view);
    }
}
