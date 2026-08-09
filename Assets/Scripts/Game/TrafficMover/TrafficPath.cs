using UnityEngine;

public class TrafficPath : MonoBehaviour
{
    [SerializeField] private Transform[] _points;

    public Transform[] Points => _points;

    public Vector3[] GetPositions()
    {
        if (_points == null || _points.Length == 0)
            return System.Array.Empty<Vector3>();

        var positions = new Vector3[_points.Length];
        for (int i = 0; i < _points.Length; i++)
            positions[i] = _points[i].position;
        return positions;
    }

    private void OnDrawGizmos()
    {
        if (_points == null || _points.Length < 2)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < _points.Length - 1; i++)
        {
            if (_points[i] != null && _points[i + 1] != null)
                Gizmos.DrawLine(_points[i].position, _points[i + 1].position);
        }

        Gizmos.color = Color.green;
        foreach (var point in _points)
        {
            if (point != null)
                Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}
