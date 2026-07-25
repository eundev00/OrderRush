using UnityEngine;

[System.Serializable]
public class StoryDayData
{
    [SerializeField] private int _dayNumber;
    [SerializeField] private string _storyText;
    [SerializeField] private DayEventData _event = new();

    public int DayNumber => _dayNumber;
    public string StoryText => _storyText;
    public DayEventData Event => _event;
}
