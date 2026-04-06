#if UNITY_EDITOR
using UnityEngine;

[ExecuteAlways]
public class GridGizmo : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;

    [Header("Visual")]
    public Color lineColor = new Color(1f, 1f, 0f, 0.8f);
    public Color emptyColor = new Color(0f, 1f, 0f, 0.1f);

    private void OnDrawGizmos()
    {
        float offsetX = width * cellSize * 0.5f;
        float offsetZ = height * cellSize * 0.5f;
        float half = cellSize * 0.5f;

        Gizmos.color = lineColor;

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(-offsetX + half, 0f, -offsetZ + y * cellSize + half);
            Vector3 end = new Vector3(offsetX + half, 0f, -offsetZ + y * cellSize + half);
            Gizmos.DrawLine(start, end);
        }

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(-offsetX + x * cellSize + half, 0f, -offsetZ + half);
            Vector3 end = new Vector3(-offsetX + x * cellSize + half, 0f, offsetZ + half);
            Gizmos.DrawLine(start, end);
        }
    }


}
#endif