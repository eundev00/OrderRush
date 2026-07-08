using UnityEngine;
using UnityEngine.UI;

public class OrderIcon : MonoBehaviour, IUIView
{
    [NotNull][SerializeField] private Image _iconImage;

    public void SetIcon(Sprite sprite)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = sprite;
            _iconImage.gameObject.SetActive(sprite != null);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
