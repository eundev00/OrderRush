using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCompleted : PopupViewBase
{
    [NotNull][SerializeField] private TMP_Text _earnedText;
    [NotNull][SerializeField] private Button _nextButton;
    [NotNull][SerializeField] private Button _exitButton;

    public Button NextButton => _nextButton;
    public Button ExitButton => _exitButton;

    public void SetEarnedCoins(int coins)
    {
        _earnedText.text = $"{coins}";
    }
}
