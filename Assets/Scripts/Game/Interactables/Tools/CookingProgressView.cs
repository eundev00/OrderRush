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

    public void SetCookingStyle()
    {
        SetVisible(true);
        SetProgress(0);
        _fill.color = Color.green;
    }

    public void SetOverdoneStyle()
    {
        SetVisible(true);
        SetProgress(0);
        _fill.color = Color.orangeRed;
    }

    public void SetVisible(bool visible)
    {

        _view.SetActive(visible);
    }
}
