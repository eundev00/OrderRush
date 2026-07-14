using UnityEngine;

[CreateAssetMenu(fileName = "DayNightSettings", menuName = "Order Rush/Day Night Settings")]
public class DayNightSettings : ScriptableObject
{
    public DayNightProfile day = new DayNightProfile
    {
        lightColor = Color.white,
        intensity = 1f,
        postExposure = 0f,
        volumeFilter = Color.white,
        indoorIntensity = 0f,
    };

    public DayNightProfile night = new DayNightProfile
    {
        lightColor = new Color(0.84f, 0.97f, 1f),
        intensity = 0.7f,
        postExposure = -0.3f,
        volumeFilter = new Color(0.84f, 0.97f, 1f),
        indoorIntensity = 1,
    };
}

[System.Serializable]
public class DayNightProfile
{
    public Color lightColor;
    public float intensity;
    public float postExposure;
    public Color volumeFilter;
    public float indoorIntensity;

}
