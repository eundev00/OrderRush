using UnityEngine;

[CreateAssetMenu(fileName = "StreetTrafficData", menuName = "Order Rush/Street Traffic Data")]
public class StreetTrafficData : ScriptableObject
{
    [Header("Vehicle Spawn")]
    [SerializeField] private float _vehicleSpawnIntervalMin = 3f;
    [SerializeField] private float _vehicleSpawnIntervalMax = 8f;

    [Header("Vehicle Speed")]
    [SerializeField] private float _vehicleSpeedMin = 5f;
    [SerializeField] private float _vehicleSpeedMax = 10f;

    [Header("Vehicle Prefabs")]
    [SerializeField] private string[] _vehiclePrefabKeys;

    [Header("Walker Spawn")]
    [SerializeField] private float _walkerSpawnIntervalMin = 2f;
    [SerializeField] private float _walkerSpawnIntervalMax = 6f;

    [Header("Walker Speed")]
    [SerializeField] private float _walkerSpeedMin = 1f;
    [SerializeField] private float _walkerSpeedMax = 2.5f;

    [Header("Walker Prefabs")]
    [SerializeField] private string[] _walkerPrefabKeys;

    public float VehicleSpawnIntervalMin => _vehicleSpawnIntervalMin;
    public float VehicleSpawnIntervalMax => _vehicleSpawnIntervalMax;
    public float VehicleSpeedMin => _vehicleSpeedMin;
    public float VehicleSpeedMax => _vehicleSpeedMax;
    public string[] VehiclePrefabKeys => _vehiclePrefabKeys;

    public float WalkerSpawnIntervalMin => _walkerSpawnIntervalMin;
    public float WalkerSpawnIntervalMax => _walkerSpawnIntervalMax;
    public float WalkerSpeedMin => _walkerSpeedMin;
    public float WalkerSpeedMax => _walkerSpeedMax;
    public string[] WalkerPrefabKeys => _walkerPrefabKeys;
}
