using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;

public class LevelContextPresenter : ILevelContextPresenter, IDisposable
{
    private readonly LevelFactory _levelFactory;
    private readonly SpawnFactory _spawnFactory;

    private LevelContext _view;

    public int LevelNumber { get; private set; } = 1;

    public List<DiningTable> DiningTables => _view?.DiningTables ?? new List<DiningTable>();
    public ServingCounter[] ServingCounters => _view != null ? _view.ServingCounters : System.Array.Empty<ServingCounter>();
    public Counter[] KitchenCounters => _view != null ? _view.KitchenCounters : System.Array.Empty<Counter>();
    public Vector3 SpawnPosition => _view != null ? _view.SpawnPoint.position : Vector3.zero;
    public Transform[] StaffIdlePoints => _view?.StaffIdlePoints;
    public Vector3 WaitingPosition => _view != null ? _view.WaitingPoint.position : Vector3.zero;
    public Quaternion WaitingRotation => _view != null ? _view.WaitingPoint.rotation : Quaternion.identity;
    public Transform LevelTransform => _view != null ? _view.transform : null;


    public LevelContextPresenter(LevelFactory levelFactory, SpawnFactory spawnFactory)
    {
        _levelFactory = levelFactory;
        _spawnFactory = spawnFactory;
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

    public async UniTask AddTableFromEffect(TableAdditionEffect effect)
    {
        if (_view == null)
        {
            Debug.LogError("[LevelContextPresenter] Cannot add table: Level context not loaded");
            return;
        }

        Transform spawnPoint = _view.GetNextTableSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("No more table spawn points available");
            return;
        }

        var table = await _spawnFactory.CreatePersistent<DiningTable>(
            effect.TablePrefabName,
            spawnPoint.position,
            spawnPoint);

        if (table != null)
        {
            table.transform.rotation = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
            _view.AddDiningTable(table);
        }
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
