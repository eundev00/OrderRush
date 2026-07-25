using Cysharp.Threading.Tasks;

public class StoryFlowService : IStoryFlowService
{
    private readonly IPopupService _popupService;
    private readonly IDayProgressService _dayProgressService;

    public StoryFlowService(
        IPopupService popupService,
        IDayProgressService dayProgressService)
    {
        _popupService = popupService;
        _dayProgressService = dayProgressService;
    }

    public async UniTask ShowStoryForDayAsync(int dayNumber)
    {
        var day = _dayProgressService.CurrentDaysData?.GetStoryForDay(dayNumber);
        if (day == null || string.IsNullOrWhiteSpace(day.StoryText))
            return;

        await _popupService.Open<PopupStoryPresenter, StoryDayData>(PrefabKeys.PopupStory, day);
    }
}
