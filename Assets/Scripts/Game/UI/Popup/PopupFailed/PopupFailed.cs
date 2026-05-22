using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupDayFailed : MonoBehaviour
{
    [NotNull][SerializeField] private Button _restartButton;
    [NotNull][SerializeField] private Button _exitButton;

    public Button RestartButton => _restartButton;
    public Button ExitButton => _exitButton;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
