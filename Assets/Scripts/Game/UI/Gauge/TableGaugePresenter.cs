using UnityEngine;

public class TableGaugePresenter : BaseUIPresenter<GaugeView>
{
    public TableGaugePresenter(Camera mainCamera, RectTransform canvasRectTransform,
        GaugeView view, Transform target, Vector3 offset)
        : base(mainCamera, canvasRectTransform, view, target, offset)
    {
    }

    public virtual void SetProgress(float progress)
    {
        if (_isDisposed || View == null) return;
        View.SetProgress(progress);
    }
}
