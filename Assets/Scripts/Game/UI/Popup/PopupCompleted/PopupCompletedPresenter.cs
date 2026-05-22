using System;
using OrderRush.Services;
using VContainer.Unity;

public class PopupCompletedPresenter : IStartable, IDisposable
{
    private readonly PopupCompleted _popup;
    private readonly IDayProgressService _dayProgressService;

    public PopupCompletedPresenter(PopupCompleted popup, IDayProgressService dayProgressService)
    {
        _popup = popup;
        _dayProgressService = dayProgressService;
    }

    public void Start()
    {
        _popup.Hide();
        _popup.NextButton.onClick.AddListener(OnNextButtonClicked);
        _popup.ExitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void ShowPopup()
    {
        var earnedCoins = _dayProgressService.CurrentDayContext.EarnedCoins.Value;
        _popup.SetEarnedCoins(earnedCoins);
        _popup.Show();
    }

    public void HidePopup()
    {
        _popup.Hide();
    }

    public void OnNextButtonClicked()
    {
    }

    public void OnExitButtonClicked()
    {
    }

    public void Dispose()
    {
        _popup.NextButton.onClick.RemoveListener(OnNextButtonClicked);
        _popup.ExitButton.onClick.RemoveListener(OnExitButtonClicked);
    }
}
