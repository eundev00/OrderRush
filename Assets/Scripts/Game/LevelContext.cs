using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelContext : MonoBehaviour
{
    [NotNull][SerializeField] Transform _interactablesRoot;
    [NotNull][SerializeField] Transform[] _tablePoints;
    [NotNull][SerializeField] Transform _spawnPoint;
    [NotNull][SerializeField] Transform _waitingPoint;
    [NotNull][SerializeField] Transform[] _staffIdlePoints;

    public List<DiningTable> DiningTables { get; private set; }
    public ServingCounter[] ServingCounters { get; private set; }
    public Counter[] KitchenCounters { get; private set; }
    public Transform SpawnPoint => _spawnPoint;
    public Transform WaitingPoint => _waitingPoint;
    public Transform[] StaffIdlePoints => _staffIdlePoints;

    void Awake()
    {
        DiningTables = new List<DiningTable>(_interactablesRoot.GetComponentsInChildren<DiningTable>());

        ServingCounters = _interactablesRoot.GetComponentsInChildren<ServingCounter>();

        KitchenCounters = _interactablesRoot.GetComponentsInChildren<Counter>()
            .Where(c => c is not ServingCounter)
            .ToArray();
    }

    public Transform GetNextTableSpawnPoint()
    {
        int index = DiningTables.Count;
        if (index < 0 || index >= _tablePoints.Length)
            return null;

        Debug.Log($" GetNextTableSpawnPoint : {index} / {_tablePoints.Length}");
        return _tablePoints[index];
    }

    public void AddDiningTable(DiningTable table)
    {
        DiningTables.Add(table);
        Debug.Log($" AddDiningTable : {DiningTables.Count} tables");
    }
}
