
using UnityEngine;

[ExecuteAlways]
public class DayNightPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light _outdoorLight;
    [SerializeField] private Light _indoorLight;

    [Header("Settings")]
    [SerializeField] private DayNightSettings _settings;

    [Header("Preview")]
    [Range(0f, 1f)]
    [SerializeField] private float _timeOfDay;

    private void OnValidate()
    {
        Apply(_timeOfDay);
    }

    private void Apply(float t)
    {
        if (_settings == null) return;

        if (_outdoorLight != null)
        {
            _outdoorLight.color = Color.Lerp(_settings.outdoorDay.color, _settings.outdoorNight.color, t);
            _outdoorLight.intensity = Mathf.Lerp(_settings.outdoorDay.intensity, _settings.outdoorNight.intensity, t);
        }

        Color ambientColor = Color.Lerp(_settings.outdoorDay.color, _settings.outdoorNight.color, t);
        float ambientIntensity = Mathf.Lerp(_settings.outdoorDay.intensity, _settings.outdoorNight.intensity, t);
        RenderSettings.ambientLight = ambientColor * ambientIntensity;

        if (_indoorLight != null)
        {
            _indoorLight.color = Color.Lerp(_settings.indoorDay.color, _settings.indoorNight.color, t);
            _indoorLight.intensity = Mathf.Lerp(_settings.indoorDay.intensity, _settings.indoorNight.intensity, t);
        }
    }
}