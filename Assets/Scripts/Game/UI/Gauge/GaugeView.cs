using UnityEngine;
using UnityEngine.UI;

public class GaugeView : MonoBehaviour, IUIView
{
    [NotNull][SerializeField] private Image _fillImage;
    [SerializeField] private GameObject _warning;

    private void Awake()
    {
        SetWarning(false);
    }

    public void SetProgress(float value)
    {
        _fillImage.fillAmount = Mathf.Clamp01(value);
    }

    public void SetWarning(bool isShow)
    {
        if (_warning) _warning.SetActive(isShow);
    }


    public void Show()
    {
        SetWarning(false);
        gameObject.SetActive(true);
        _fillImage.fillAmount = 0f;
    }

    public void Hide()
    {
        SetWarning(false);
        gameObject.SetActive(false);

    }

}
