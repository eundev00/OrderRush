using UnityEngine;

public class TrafficContext : MonoBehaviour
{
    [SerializeField] private TrafficPath[] _roadPaths;
    [SerializeField] private Transform[] _walkerBoundaryPoints;

    public TrafficPath[] RoadPaths => _roadPaths;
    public Transform[] WalkerBoundaryPoints => _walkerBoundaryPoints;
}
