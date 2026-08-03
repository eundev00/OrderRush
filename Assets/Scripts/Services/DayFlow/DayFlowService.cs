using System;
using Cysharp.Threading.Tasks;

public class DayFlowService : IDayFlowService
{
    private readonly IDayProgressService _dayProgressService;
    private readonly IPopupService _popupService;
    private readonly IStoryFlowService _storyFlowService;
    private readonly ICameraDirector _cameraDirector;
    private readonly ICustomerService _customerService;
    private readonly StaffManager _staffManager;
    private readonly ISceneTransitionService _sceneTransition;

    public DayFlowService(
        IDayProgressService dayProgressService,
        IPopupService popupService,
        IStoryFlowService storyFlowService,
        ICameraDirector cameraDirector,
        ICustomerService customerService,
        StaffManager staffManager,
        ISceneTransitionService sceneTransition)
    {
        _dayProgressService = dayProgressService;
        _popupService = popupService;
        _storyFlowService = storyFlowService;
        _cameraDirector = cameraDirector;
        _customerService = customerService;
        _staffManager = staffManager;
        _sceneTransition = sceneTransition;
    }

    public async UniTask RunFirstDayAsync(int dayNumber)
    {
        _dayProgressService.InitDay(dayNumber);
        _customerService.Initialize();
        _staffManager.Initialize();

        try
        {
            await _storyFlowService.ShowStoryForDayAsync(dayNumber);
            await _cameraDirector.PlayDayIntroAsync();
        }
        catch (OperationCanceledException)
        {
            return;
        }

        _dayProgressService.StartDayTimer();
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
        }
    }

    private async UniTask HandleDayCompletedAsync(DayEndedEvent evt)
    {
        int completedDay = evt.NextDay - 1;
        var data = new DayCompletedData(completedDay, evt.EarnedCoins);
        var action = await _popupService.Open<PopupCompletedPresenter, DayCompletedData, DayCompletedAction>(
            PrefabKeys.PopupCompleted, data);

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
        int enteringDay = _dayProgressService.CurrentDayContext.DayNumber.Value;

        await _storyFlowService.ShowStoryForDayAsync(enteringDay);
        await _popupService.Open<PopupCardShopPresenter>(PrefabKeys.PopupCardShop);
        await _cameraDirector.PlayDayIntroAsync();

        _dayProgressService.StartDayTimer();
    }

    private void ExitToLobby()
    {
        _dayProgressService.CompleteRun();
        _customerService.Dispose();

        _sceneTransition.TransitionAsync("GameplayScene", "LobbyScene").Forget();
    }
}
