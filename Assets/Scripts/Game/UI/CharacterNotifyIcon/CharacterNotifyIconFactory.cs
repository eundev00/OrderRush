using UnityEngine;

public class CharacterNotifyIconFactory : BaseUIFactory<CharacterNotifyIconView, CharacterNotifyIconPresenter>
{
    public CharacterNotifyIconFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.CharacterNotifyIcon)
    {
    }

    public override CharacterNotifyIconPresenter Create(Transform target, Vector3 offset, Sprite icon = null)
    {
        CharacterNotifyIconView view = GetViewFromPool();
        var presenter = new CharacterNotifyIconPresenter(_camera, _canvasRectTransform, view, target, offset, icon);
        return presenter;
    }

    protected override void OnReleaseView(CharacterNotifyIconView view)
    {
        base.OnReleaseView(view);
        view.SetIcon(null); // 아이콘 정리
    }
}
