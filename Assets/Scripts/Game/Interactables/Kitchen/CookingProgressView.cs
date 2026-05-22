using UnityEngine;
using UnityEngine.UI;

public class CookingProgressView : MonoBehaviour
{
    [NotNull][SerializeField] Image _fill;
    [NotNull][SerializeField] GameObject _view;

    void Awake()
    {
        SetVisible(false);
    }

    public void SetProgress(float value)
    {
        _fill.fillAmount = value;
    }

    public void SetVisible(bool visible)
    {
        _view.SetActive(visible);
        if (visible) SetProgress(1.0f);
    }
}
