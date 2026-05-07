using UnityEngine;

public class TableGaugeFactory : BaseUIFactory<GaugeView, TableGaugePresenter>
{
    public TableGaugeFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.TableGauge)
    {
    }

    public override TableGaugePresenter Create(Transform target, Vector3 offset, Sprite icon = null)
    {
        GaugeView view = GetViewFromPool();
        var presenter = new TableGaugePresenter(_camera, _canvasRectTransform, view, target, offset, icon);
        return presenter;
    }

    protected override void OnReleaseView(GaugeView view)
    {
        base.OnReleaseView(view);
        view.SetIcon(null);
    }
}
