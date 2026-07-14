using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class DayNightPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light _directionalLight;
    [SerializeField] private Volume _globalVolume;
    [SerializeField] private Transform _indoorLightsParent;

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
        if (_settings == null)
            return;

        DayNightProfile day = _settings.day;
        DayNightProfile night = _settings.night;

        if (_directionalLight != null)
        {
            _directionalLight.color = Color.Lerp(day.lightColor, night.lightColor, t);
            _directionalLight.intensity = Mathf.Lerp(day.intensity, night.intensity, t);
        }

        if (_globalVolume != null &&
            _globalVolume.profile.TryGet<ColorAdjustments>(out var ca))
        {
            ca.postExposure.Override(Mathf.Lerp(day.postExposure, night.postExposure, t));
            ca.colorFilter.Override(Color.Lerp(day.volumeFilter, night.volumeFilter, t));
        }

        if (_indoorLightsParent != null)
        {
            float indoorIntensity = Mathf.Lerp(day.indoorIntensity, night.indoorIntensity, t);
            foreach (var light in _indoorLightsParent.GetComponentsInChildren<Light>())
            {
                if (light != null)
                    light.intensity = indoorIntensity;
            }
        }
    }
}
