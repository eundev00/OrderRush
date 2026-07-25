using UnityEngine;

[System.Serializable]
public class DayEventData
{
    [SerializeField] private bool _isRainy;

    public bool IsRainy => _isRainy;
}
