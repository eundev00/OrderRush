using System;
using OrderRush.Services;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class PopupFailedPresenter : IStartable, IDisposable
{
    private readonly PopupDayFailed _popup;
    private readonly IDayProgressService _dayProgressService;

    public PopupFailedPresenter(
        PopupDayFailed popup,
        IDayProgressService dayProgressService)
    {
        _popup = popup;
        _dayProgressService = dayProgressService;
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
        _popup.Hide();
        _dayProgressService.RestartDay();
    }

    public void OnExitButtonClicked()
    {
        SceneManager.UnloadSceneAsync("GameplayScene");
        SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }

    public void Dispose()
    {
        _popup.RestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        _popup.ExitButton.onClick.RemoveListener(OnExitButtonClicked);
    }
}
