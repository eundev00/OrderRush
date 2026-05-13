using UnityEngine;

public class CharacterEmoteIconFactory : BaseUIFactory<CharacterEmoteIconView, CharacterEmoteIconPresenter>
{
    public CharacterEmoteIconFactory(RectTransform canvasRectTransform)
        : base(canvasRectTransform, PrefabKeys.CharacterEmoteIcon)
    {
    }

    public override CharacterEmoteIconPresenter Create(Transform target, Vector3 offset)
    {
        CharacterEmoteIconView view = GetViewFromPool();
        var presenter = new CharacterEmoteIconPresenter(_camera, _canvasRectTransform, view, target, offset);
        return presenter;
    }

    protected override void OnReleaseView(CharacterEmoteIconView view)
    {
        base.OnReleaseView(view);
    }
}
