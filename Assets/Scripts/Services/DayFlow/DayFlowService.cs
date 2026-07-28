using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class DayFlowService : IDayFlowService
{
    private readonly IDayProgressService _dayProgressService;
    private readonly IPopupService _popupService;
    private readonly IStoryFlowService _storyFlowService;
    private readonly ICameraDirector _cameraDirector;
    private readonly ICustomerService _customerService;
    private readonly StaffManager _staffManager;

    public DayFlowService(
        IDayProgressService dayProgressService,
        IPopupService popupService,
        IStoryFlowService storyFlowService,
        ICameraDirector cameraDirector,
        ICustomerService customerService,
        StaffManager staffManager)
    {
        _dayProgressService = dayProgressService;
        _popupService = popupService;
        _storyFlowService = storyFlowService;
        _cameraDirector = cameraDirector;
        _customerService = customerService;
        _staffManager = staffManager;
    }

    public async UniTask RunFirstDayAsync(int dayNumber)
    {
        try
        {
            await _storyFlowService.ShowStoryForDayAsync(dayNumber);
            await _cameraDirector.PlayDayIntroAsync();
        }
        catch (OperationCanceledException)
        {
            return;
        }

        _dayProgressService.InitDay(dayNumber);
        _customerService.Initialize();
        _staffManager.Initialize();
    }

    public async UniTask HandleDayEndedAsync(DayEndedEvent evt)
    {
        try
        {
            await _cameraDirector.PlayDayOutroAsync();

            if (evt.IsCompleted)
                await HandleDayCompletedAsync(evt);
            else
                await HandleDayFailedAsync();
        }
        catch (OperationCanceledException)
        {
            // 씬 언로드로 카메라 연출 취소됨
        }
    }

    private async UniTask HandleDayCompletedAsync(DayEndedEvent evt)
    {
        var action = await _popupService.Open<PopupCompletedPresenter, int, DayCompletedAction>(
            PrefabKeys.PopupCompleted, evt.EarnedCoins);

        switch (action)
        {
            case DayCompletedAction.Next:
                await StartNextDayAsync();
                break;
            case DayCompletedAction.Exit:
                ExitToLobby();
                break;
        }
    }

    private async UniTask HandleDayFailedAsync()
    {
        var action = await _popupService.Open<PopupFailedPresenter, DayFailedAction>(
            PrefabKeys.PopupFailed);

        switch (action)
        {
            case DayFailedAction.Restart:
                await _cameraDirector.PlayDayIntroAsync();
                _dayProgressService.RestartDay();
                break;
            case DayFailedAction.Exit:
                ExitToLobby();
                break;
        }
    }

    private async UniTask StartNextDayAsync()
    {
        _dayProgressService.SetNextDay();
        int enteringDay = _dayProgressService.CurrentDayContext.DayNumber;

        await _storyFlowService.ShowStoryForDayAsync(enteringDay);
        await _popupService.Open<PopupCardShopPresenter>(PrefabKeys.PopupCardShop);
        await _cameraDirector.PlayDayIntroAsync();

        _dayProgressService.StartDayTimer();
    }

    private void ExitToLobby()
    {
        SceneManager.UnloadSceneAsync("GameplayScene");
        SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }
}
