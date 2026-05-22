using UnityEngine;

public class TableGaugeFactory : BaseUIFactory<GaugeView, TableGaugePresenter>
{
    public TableGaugeFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.TableGauge)
    {
    }

    protected override TableGaugePresenter CreatePresenter(Transform target, Vector3 offset)
    {
        GaugeView view = GetViewFromPool();
        var presenter = new TableGaugePresenter(_camera, _canvasRectTransform, view, target, offset);
        return presenter;
    }

    protected override void OnReleaseView(GaugeView view)
    {
        base.OnReleaseView(view);
    }
}
