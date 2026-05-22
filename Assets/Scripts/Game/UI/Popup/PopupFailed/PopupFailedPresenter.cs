using System;
using VContainer.Unity;

public class PopupFailedPresenter : IStartable, IDisposable
{
    private readonly PopupDayFailed _popup;

    public PopupFailedPresenter(PopupDayFailed popup)
    {
        _popup = popup;
    }

    public void Start()
    {
        _popup.Hide();
        _popup.RestartButton.onClick.AddListener(OnRestartButtonClicked);
        _popup.ExitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void ShowPopup()
    {
        _popup.Show();
    }

    public void HidePopup()
    {
        _popup.Hide();
    }

    public void OnRestartButtonClicked()
    {
    }

    public void OnExitButtonClicked()
    {
    }

    public void Dispose()
    {
        _popup.RestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        _popup.ExitButton.onClick.RemoveListener(OnExitButtonClicked);
    }
}
