using UnityEngine;

public class OrderIconFactory : BaseUIFactory<OrderIconView, OrderIconPresenter>
{
    public OrderIconFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.OrderIcon)
    {
    }

    public override OrderIconPresenter Create(Transform target, Vector3 offset, Sprite icon = null)
    {
        OrderIconView view = GetViewFromPool();
        var presenter = new OrderIconPresenter(_camera, _canvasRectTransform, view, target, offset, icon);
        return presenter;
    }

    protected override void OnReleaseView(OrderIconView view)
    {
        base.OnReleaseView(view);
        view.SetIcon(null); // 아이콘 정리
    }
}
