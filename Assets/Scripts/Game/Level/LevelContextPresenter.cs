using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;

public class LevelContextPresenter : ILevelContextPresenter, IDisposable
{
    private readonly LevelFactory _levelFactory;

    private LevelContext _view;

    public int LevelNumber { get; private set; } = 1;

    public IReadOnlyList<DiningTable> DiningTables => _view?.DiningTables ?? Array.Empty<DiningTable>();
    public Vector3 SpawnPosition => _view != null ? _view.SpawnPoint.position : Vector3.zero;
    public Vector3 WaitingPosition => _view != null ? _view.WaitingPoint.position : Vector3.zero;
    public Quaternion WaitingRotation => _view != null ? _view.WaitingPoint.rotation : Quaternion.identity;
    public Transform LevelTransform => _view != null ? _view.transform : null;


    public LevelContextPresenter(LevelFactory levelFactory)
    {
        _levelFactory = levelFactory;
    }

    public async UniTask LoadLevelContext(int levelNumber)
    {
        LevelNumber = levelNumber;

        var levelContext = await _levelFactory.CreateLevelContext(levelNumber);
        if (levelContext == null)
        {
            Debug.LogError($"Failed to load level map: Level{levelNumber}");
            return;
        }
        _view = levelContext;
    }

    public void Dispose()
    {
        if (_view != null)
        {
            _levelFactory.ReleaseLevelContext(LevelNumber);
            _view = null;
        }
    }
}
