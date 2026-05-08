using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

public class CustomerService : ICustomerService, ITickable
{
    private LevelContext _levelContext;
    private List<RecipeData> _availableRecipes;
    private float _spawnInterval;
    private readonly SpawnFactory _spawnFactory;
    private readonly ILevelProgressService _levelProgressService;
    private float _timer;
    private int _spawnCount;

    public CustomerService(SpawnFactory spawnFactory, ILevelProgressService levelProgressService)
    {
        _spawnFactory = spawnFactory;
        _levelProgressService = levelProgressService;
    }

    public void SetTables(LevelContext levelContext, LevelData levelData)
    {
        _levelContext = levelContext;
        _availableRecipes = levelData.AvailableRecipes;
        _spawnInterval = levelData.CustomerSpawnInterval;
        _spawnCount = levelData.MaxCustomers;
    }

    public async void Tick()
    {
        if (_spawnCount == 0) return;
        if (_levelContext == null) return;

        _timer += Time.deltaTime;

        if (_timer >= _spawnInterval)
        {
            _timer = 0;
            int count = await TrySpawn();
            _spawnCount -= count;
        }
    }

    private async UniTask<int> TrySpawn()
    {
        var table = _levelContext.DiningTables.FirstOrDefault(t => t.IsEmptyTable());
        if (table == null) return 0;

        int seatIndex = 0;
        var customer = await _spawnFactory.Create<CustomerCharacter>(PrefabKeys.GetPrefabPath(PrefabKeys.CustomerCharacter1));
        customer.transform.SetParent(_levelContext.transform);
        customer.SetSpawnPosition(_levelContext.SpawnPoint.position);
        customer.WarpTo(_levelContext.SpawnPoint.position);
        customer.EnqueueGoToSeat(table, seatIndex);
        seatIndex++;

        return seatIndex;
    }


}
