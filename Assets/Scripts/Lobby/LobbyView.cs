using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : MonoBehaviour
{
    [NotNull][SerializeField] private Button _newGameButton;
    [NotNull][SerializeField] private Button _continueButton;
    [NotNull][SerializeField] private TMP_Text _dayText;
    [NotNull][SerializeField] private TMP_Text _coinText;

    [SerializeField] private float _buttonShowDelay = 0.15f;

    public Button NewGameButton => _newGameButton;
    public Button ContinueButton => _continueButton;
    public bool CanContinue { get; set; }


    private async void Start()
    {
        await UniTask.Yield();

        HideBoth();

        if (CanContinue)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_buttonShowDelay));
            _continueButton.gameObject.SetActive(true);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_buttonShowDelay));
        _newGameButton.gameObject.SetActive(true);
    }

    public void SetDay(int day)
    {
        _dayText.text = $"Day {day}";
    }

    public void SetCoins(int coins)
    {
        _coinText.text = $"{coins}";
    }

    public void HideBoth()
    {
        _newGameButton.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(false);
    }

}