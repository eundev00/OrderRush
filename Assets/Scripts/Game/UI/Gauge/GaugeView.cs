using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GaugeView : MonoBehaviour, IUIView
{
    [NotNull][SerializeField] private Image _fillImage;
    [SerializeField] private Image _icon;


    private void Awake()
    {
        if (_icon != null)
        {
            _icon.gameObject.SetActive(false);
        }
    }

    public void SetProgress(float value)
    {
        _fillImage.fillAmount = Mathf.Clamp01(value);
    }

    public void SetColor(Color color)
    {
        _fillImage.color = color;
    }

    public void SetIcon(Sprite sprite)
    {
        if (_icon != null)
        {
            _icon.sprite = sprite;
            _icon.gameObject.SetActive(sprite != null);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _fillImage.fillAmount = 0f;

    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (_icon != null)
        {
            _icon.gameObject.SetActive(false);
        }
    }

}
