using System;
using Services.UpdateService;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TableGauge : MonoBehaviour, IUpdatable
{
    [NotNull][SerializeField] private Image _fillImage;
    [NotNull][SerializeField] private GameObject _bellIcon;
    [NotNull][SerializeField] private GameObject _clockIcon;

    private IUpdateSubscriptionService _updateService;
    private IGameDataService _gameDataService;
    private ISoundService _soundService;

    private float _elapsedWaitTime;
    private bool _isWaitingForOrder;
    private bool _isWaitingFood;
    private float _waitDuration;

    public event Action OnTimeout;

    [Inject]
    public void Construct(
        IUpdateSubscriptionService updateService,
        IGameDataService gameDataService,
        ISoundService soundService)
    {
        _updateService = updateService;
        _gameDataService = gameDataService;
        _soundService = soundService;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _fillImage.fillAmount = 1f;
    }

    public void Hide()
    {
        _bellIcon?.SetActive(false);
        _clockIcon?.SetActive(false);
        gameObject.SetActive(false);
    }

    private void SetProgress(float value)
    {
        _fillImage.fillAmount = Mathf.Clamp01(value);
    }

    public void StartGauge()
    {
        _elapsedWaitTime = 0f;
        _isWaitingForOrder = true;
        _isWaitingFood = false;
        _waitDuration = _gameDataService.Config.FoodWaitDuration;

        _soundService?.PlaySfx(AudioKeys.bell1);

        _bellIcon?.SetActive(true);
        _clockIcon?.SetActive(false);

        Show();

        _updateService.RegisterUpdatable(this);
    }

    public void StopGauge()
    {
        _isWaitingForOrder = false;
        _isWaitingFood = false;
        _elapsedWaitTime = 0f;

        _bellIcon?.SetActive(false);
        _clockIcon?.SetActive(false);

        _updateService.UnregisterUpdatable(this);
        Hide();
    }

    public void SwitchToFoodWaiting()
    {
        _isWaitingFood = true;
        _elapsedWaitTime = 0f;
        SetProgress(1f);

        _bellIcon?.SetActive(false);
        _clockIcon?.SetActive(true);
    }

    public void ExtendTime()
    {
        if (!_isWaitingFood)
        {
            Debug.LogWarning("[TableGauge] Cannot extend time during order waiting phase");
            return;
        }

        float recoverAmount = _waitDuration * _gameDataService.Config.PatienceRecoveryAmount;
        _elapsedWaitTime = Mathf.Max(0, _elapsedWaitTime - recoverAmount);
        float newProgress = 1f - (_elapsedWaitTime / _waitDuration);
        SetProgress(newProgress);
    }

    public void ManagedUpdate()
    {
        if (!_isWaitingForOrder) return;

        _elapsedWaitTime += Time.deltaTime;
        float progress = 1f - (_elapsedWaitTime / _waitDuration);

        SetProgress(progress);

        if (_elapsedWaitTime >= _waitDuration)
        {
            OnTimeout?.Invoke();
        }
    }

    void OnDestroy()
    {
        _updateService?.UnregisterUpdatable(this);
    }
}
