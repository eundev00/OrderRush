using UnityEngine;

public class LevelContext : MonoBehaviour
{
    [SerializeField] DiningTable[] _diningTables;
    [SerializeField] Transform _spawnPoint;
    [SerializeField] Transform _waitingPoint;

    public DiningTable[] DiningTables => _diningTables;
    public Transform SpawnPoint => _spawnPoint;
    public Transform WaitingPoint => _waitingPoint;
}
