using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupStoryView : PopupViewBase
{
    [NotNull][SerializeField] private TMP_Text _dayText;
    [NotNull][SerializeField] private TMP_Text _storyText;
    [NotNull][SerializeField] private Button _continueButton;

    public Button ContinueButton => _continueButton;

    public void SetDayNumber(int dayNumber)
    {
        _dayText.text = $"Day {dayNumber}";
    }

    public void SetText(string text)
    {
        _storyText.text = text;
    }
}
