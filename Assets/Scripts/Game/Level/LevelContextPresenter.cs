using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

public class LevelContextPresenter : ILevelContextPresenter, ITickable, IDisposable
{
    private readonly IResourcesLoaderService _resourcesLoaderService;
    private readonly ILevelsDataService _levelsDataService;
    private readonly IOrderService _orderService;
    private readonly LevelFactory _levelFactory;

    private LevelData _currentLevelData;
    private GameObject _currentLevelMap;
    private int _currentMoney;
    private float _elapsedTime;
    private bool _isLevelActive;

    public LevelContextPresenter(IResourcesLoaderService resourcesLoaderService,
        ILevelsDataService levelsDataService,
        IOrderService orderService)
    {
        _resourcesLoaderService = resourcesLoaderService;
        _levelsDataService = levelsDataService;
        _orderService = orderService;
        _levelFactory = new LevelFactory(resourcesLoaderService);
    }

    public async UniTask LoadLevelContext(int levelNumber)
    {
        _isLevelActive = false;

        // 1. 레벨 데이터 가져오기
        _currentLevelData = _levelsDataService.GetLevelData(levelNumber);
        if (_currentLevelData == null)
        {
            Debug.LogError($"Level {levelNumber} data not found!");
            return;
        }

        // 2. 레벨 맵 로드 (Addressables로 로드, LevelContext로 타입 체크)
        // TODO: Addressables 로드 구현

        // 3. 초기화
        _currentMoney = 0;
        _elapsedTime = 0f;
        _isLevelActive = true;

        Debug.Log($"Level {_currentLevelData.LevelNumber} loaded: {_currentLevelData.LevelName}");
    }

    public void AddMoney(int amount)
    {
        if (!_isLevelActive) return;

        _currentMoney += amount;

        CheckLevelCompleted();
    }

    public void Tick()
    {
        if (!_isLevelActive || _currentLevelData == null) return;

        _elapsedTime += Time.deltaTime;

        // 시간 제한 체크
        if (_currentLevelData.TimeLimit > 0 && _elapsedTime >= _currentLevelData.TimeLimit)
        {
            CheckLevelFailed();
        }
    }

    private void CheckLevelCompleted()
    {
        if (_currentMoney >= _currentLevelData.TargetMoney)
        {
            _isLevelActive = false;
            _levelsDataService.SetMaxReachedLevel(_currentLevelData.LevelNumber + 1);
            Debug.Log($"Level {_currentLevelData.LevelNumber} completed!");
        }
    }

    private void CheckLevelFailed()
    {
        if (_currentMoney < _currentLevelData.TargetMoney)
        {
            _isLevelActive = false;
            Debug.Log($"Level {_currentLevelData.LevelNumber} failed!");
        }
    }

    public void Dispose()
    {
        // TODO: Addressables 리소스 해제
        if (_currentLevelMap != null)
        {
            // _levelFactory.ReleaseLevelMap(_currentLevelMap);
        }
    }
}
