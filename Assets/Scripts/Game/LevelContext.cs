using UnityEngine;

public class LevelContext : MonoBehaviour
{
    [NotNull][SerializeField] DiningTable[] _diningTables;
    [NotNull][SerializeField] Transform _spawnPoint;
    [NotNull][SerializeField] Transform _waitingPoint;

    public DiningTable[] DiningTables => _diningTables;
    public Transform SpawnPoint => _spawnPoint;
    public Transform WaitingPoint => _waitingPoint;
}
