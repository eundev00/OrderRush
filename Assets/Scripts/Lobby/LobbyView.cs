using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _dayText;

    public Button StartButton => _startButton;

    public void SetDay(int day)
    {
        _dayText.text = $"Day {day}";
    }
}