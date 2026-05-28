using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class LevelContext : MonoBehaviour
{
    [NotNull][SerializeField] DiningTable[] _diningTables;
    [NotNull][SerializeField] Transform _spawnPoint;
    [NotNull][SerializeField] Transform _waitingPoint;

    private List<DiningTable> _diningTablesList;

    void Awake()
    {
        _diningTablesList = new List<DiningTable>(_diningTables);
    }

    public IReadOnlyList<DiningTable> DiningTables => _diningTablesList;
    public Transform SpawnPoint => _spawnPoint;
    public Transform WaitingPoint => _waitingPoint;

    public void AddDiningTable(DiningTable table)
    {
        _diningTablesList.Add(table);
    }
}
