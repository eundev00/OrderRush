using UnityEngine;

public class OrderIconPresenter : BaseUIPresenter<OrderIconView>
{
    public OrderIconPresenter(Camera mainCamera, RectTransform canvasRectTransform, OrderIconView view, Transform target, Vector3 offset)
        : base(mainCamera, canvasRectTransform, view, target, offset)
    {
    }

    public void SetIcon(Sprite sprite)
    {
        if (_isDisposed || View == null) return;
        View.SetIcon(sprite);
    }
}
