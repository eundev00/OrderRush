using UnityEngine;
using UnityEngine.UI;

public class CookingGauge : MonoBehaviour
{
    [NotNull][SerializeField] private Image _fillImage;
    [SerializeField] private GameObject _warning;

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
