using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCompleted : PopupViewBase
{
    [NotNull][SerializeField] private TMP_Text _dayText;
    [NotNull][SerializeField] private TMP_Text _earnedText;
    [NotNull][SerializeField] private Button _nextButton;
    [NotNull][SerializeField] private Button _exitButton;

    public Button NextButton => _nextButton;
    public Button ExitButton => _exitButton;

    public void SetDayText(int dayNumber)
    {
        _dayText.text = $"Day {dayNumber} Complete!";
    }

    public void SetEarnedCoins(int coins)
    {
        _earnedText.text = $"{coins}";
    }
}
