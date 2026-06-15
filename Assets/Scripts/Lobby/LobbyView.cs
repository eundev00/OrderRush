using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private TMP_Text _dayText;
    [SerializeField] private TMP_Text _coinText;

    public Button StartButton => _startButton;
    public Button ResetButton => _resetButton;

    public void SetDay(int day)
    {
        _dayText.text = $"Day {day}";
    }

    public void SetCoins(int coins)
    {
        _coinText.text = $"{coins}";
    }
}