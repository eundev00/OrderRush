using UnityEngine;

public class CharacterEmoteIconPresenter : BaseUIPresenter<CharacterEmoteIconView>
{
    public CharacterEmoteIconPresenter(Camera mainCamera, RectTransform canvasRectTransform, CharacterEmoteIconView view, Transform target, Vector3 offset, Sprite icon = null)
        : base(mainCamera, canvasRectTransform, view, target, offset)
    {
        if (icon != null)
        {
            View.SetIcon(icon);
        }
    }

    public void SetIcon(Sprite sprite)
    {
        if (_isDisposed || View == null) return;
        View.SetIcon(sprite);
    }
}
