using UnityEngine;

namespace OrderRush.Data
{
    [System.Serializable]
    public class StoryPhaseData
    {
        [SerializeField] private int _dayStart;
        [SerializeField] private string _storyText;
        [SerializeField] private string _mood;

        public int DayStart => _dayStart;
        public string StoryText => _storyText;
        public string Mood => _mood;
    }
}
