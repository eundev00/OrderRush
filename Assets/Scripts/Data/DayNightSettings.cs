using UnityEngine;

[CreateAssetMenu(fileName = "DayNightSettings", menuName = "Order Rush/Day Night Settings")]
public class DayNightSettings : ScriptableObject
{
    [Header("Default Layer (Outdoor)")]
    public LightState outdoorDay = new LightState
    {
        // 베이지 톤 (#E4D5B7), 실내 Day 값과 동일
        color = new Color(0.894f, 0.835f, 0.718f),
        intensity = 1f,
    };
    public LightState outdoorNight = new LightState
    {
        color = new Color(0.84f, 0.97f, 1f),
        intensity = 0.7f,
    };

    [Header("Indoor Layer")]
    public LightState indoorDay = new LightState
    {
        // Outdoor Day와 동일한 베이지 톤
        color = new Color(0.894f, 0.835f, 0.718f),
        intensity = 1f,
    };
    public LightState indoorNight = new LightState
    {
        // 밤에는 색상 유지, 밝기만 살짝 낮춤 (실외 대비 변화폭 작게)
        color = new Color(0.894f, 0.835f, 0.718f),
        intensity = 0.75f,
    };
}

[System.Serializable]
public class LightState
{
    public Color color;
    public float intensity;
}