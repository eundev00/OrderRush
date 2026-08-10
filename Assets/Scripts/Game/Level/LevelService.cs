using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;

public class LevelService : ILevelService, IDisposable
{
    private readonly SpawnFactory _spawnFactory;
    private readonly IResourcesLoaderService _resourceLoader;
    private readonly Transform _root;
    private readonly ISubscriber<RainEvent> _rainSubscriber;
    private readonly ISubscriber<NightLightEvent> _nightLightSubscriber;
    private readonly ISoundService _soundService;

    private readonly CompositeDisposable _disposables = new();

    private LevelContext _context;
    private string _levelPath;

    public LevelContext Context => _context;

    public LevelService(
        SpawnFactory spawnFactory,
        IResourcesLoaderService resourceLoader,
        Transform root,
        ISubscriber<RainEvent> rainSubscriber,
        ISubscriber<NightLightEvent> nightLightSubscriber,
        ISoundService soundService)
    {
        _spawnFactory = spawnFactory;
        _resourceLoader = resourceLoader;
        _root = root;
        _rainSubscriber = rainSubscriber;
        _nightLightSubscriber = nightLightSubscriber;
        _soundService = soundService;
    }

    public async UniTask LoadLevelAsync(int levelNumber)
    {
        _levelPath = PrefabKeys.GetLevelPath(levelNumber);

        var context = await _spawnFactory.Create<LevelContext>(_levelPath);
        if (context == null)
        {
            Debug.LogError($"[LevelService] Failed to load level map: {_levelPath}");
            return;
        }

        context.transform.SetParent(_root, false);
        _context = context;

        SetNightLight(false);

        _rainSubscriber.Subscribe(e => SetRain(e.IsRainy)).AddTo(_disposables);
        _nightLightSubscriber.Subscribe(e => SetNightLight(e.IsNight)).AddTo(_disposables);
    }

    private void SetRain(bool on)
    {
        if (_context.RainRoot != null)
            _context.RainRoot.SetActive(on);

        if (on)
            _soundService.PlaySfx(AudioKeys.rain_light, isLoop: true, volume: 1f);
        else
            _soundService.StopSfx(AudioKeys.rain_light);
    }

    private void SetNightLight(bool on)
    {
        if (_context.NightLightRoot != null)
            _context.NightLightRoot.SetActive(on);
    }

    public void Dispose()
    {
        _disposables.Dispose();

        if (_context != null)
        {
            UnityEngine.Object.Destroy(_context.gameObject);
            _context = null;
        }

        if (!string.IsNullOrEmpty(_levelPath))
        {
            _resourceLoader.Release(_levelPath);
            _levelPath = null;
        }
    }
}
