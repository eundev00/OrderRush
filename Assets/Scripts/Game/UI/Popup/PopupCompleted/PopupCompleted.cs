using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCompleted : MonoBehaviour
{
    [NotNull][SerializeField] private TMP_Text _earnedText;
    [NotNull][SerializeField] private Button _nextButton;
    [NotNull][SerializeField] private Button _exitButton;

    public Button NextButton => _nextButton;
    public Button ExitButton => _exitButton;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetEarnedCoins(int coins)
    {
        _earnedText.text = $"{coins}";
    }
}
