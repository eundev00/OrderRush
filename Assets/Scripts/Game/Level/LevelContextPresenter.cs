using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OrderRush.Data;
using OrderRush.Services;
using UnityEngine;

public class LevelContextPresenter : ILevelContextPresenter, IDisposable
{
    private readonly LevelFactory _levelFactory;
    private readonly IAccountService _accountService;
    private readonly IGameDataService _gameDataService;
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


    public LevelContextPresenter(
        LevelFactory levelFactory,
        IAccountService accountService,
        IGameDataService gameDataService,
        SpawnFactory spawnFactory)
    {
        _levelFactory = levelFactory;
        _accountService = accountService;
        _gameDataService = gameDataService;
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

        await RestorePurchasedTables();
    }

    private async UniTask RestorePurchasedTables()
    {
        var purchasedCardIDs = _accountService.GetPurchasedCardIDs();

        foreach (var cardID in purchasedCardIDs)
        {
            Debug.Log($" RestorePurchasedTables : {cardID}");
            var card = _gameDataService.GetCardByID(cardID);
            if (card?.Effect == null)
                continue;

            switch (card.Effect.EffectType)
            {
                case EffectType.Table:
                    await AddTableFromEffect((TableAdditionEffect)card.Effect);
                    break;
            }
        }
    }

    private async UniTask AddTableFromEffect(TableAdditionEffect effect)
    {
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
            _view.AddDiningTable(table);
        }
    }

    public void AddDiningTable(DiningTable table)
    {
        _view?.AddDiningTable(table);
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
