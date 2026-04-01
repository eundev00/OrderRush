using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;

    private bool[,] occupied;

    private void Awake()
    {
        occupied = new bool[width, height];
    }

    private Vector2 GetOffset()
    {
        return new Vector2(
            width * cellSize * 0.5f,
            height * cellSize * 0.5f
        );
    }

    // 월드 좌표 → 그리드 좌표
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector2 offset = GetOffset();
        int x = Mathf.FloorToInt((worldPos.x + offset.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.z + offset.y) / cellSize);
        return new Vector2Int(x, y);
    }

    // 그리드 좌표 → 월드 좌표 (셀 중앙)
    public Vector3 GridToWorld(Vector2Int cell)
    {
        Vector2 offset = GetOffset();
        return new Vector3(
            -offset.x + (cell.x + 0.5f) * cellSize,
            0f,
            -offset.y + (cell.y + 0.5f) * cellSize
        );
    }

    public bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0
            && cell.x < width && cell.y < height;
    }

    public bool IsOccupied(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return true;
        return occupied[cell.x, cell.y];
    }

    public bool CanPlace(Vector2Int cell, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                var target = new Vector2Int(cell.x + x, cell.y + y);
                if (!IsInBounds(target)) return false;
                if (occupied[target.x, target.y]) return false;
            }
        return true;
    }

    public void SetOccupied(Vector2Int cell, Vector2Int size, bool value)
    {
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                var target = new Vector2Int(cell.x + x, cell.y + y);
                if (IsInBounds(target))
                    occupied[target.x, target.y] = value;
            }
    }
}