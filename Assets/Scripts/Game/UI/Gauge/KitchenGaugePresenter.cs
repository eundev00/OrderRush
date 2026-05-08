using UnityEngine;

public class KitchenGaugePresenter : BaseUIPresenter<GaugeView>
{
    public KitchenGaugePresenter(Camera mainCamera, RectTransform canvasRectTransform,
        GaugeView view, Transform target, Vector3 offset)
        : base(mainCamera, canvasRectTransform, view, target, offset)
    {
    }

    public virtual void SetProgress(float progress)
    {
        if (_isDisposed || View == null) return;
        View.SetProgress(progress);
    }

    public virtual void SetColor(Color color)
    {
        if (_isDisposed || View == null) return;
        View.SetColor(color);
    }
}
