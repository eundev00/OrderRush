using UnityEngine;

public class OrderIconFactory : BaseUIFactory<OrderIconView, OrderIconPresenter>
{
    public OrderIconFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.CharacterOrderIcon)
    {
    }

    public override OrderIconPresenter Create(Transform target, Vector3 offset)
    {
        OrderIconView view = GetViewFromPool();
        var presenter = new OrderIconPresenter(_camera, _canvasRectTransform, view, target, offset);
        return presenter;
    }

    protected override void OnReleaseView(OrderIconView view)
    {
        base.OnReleaseView(view);
        view.SetIcon(null);
    }
}
