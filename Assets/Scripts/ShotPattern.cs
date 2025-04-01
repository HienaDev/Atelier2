using UnityEngine;

[System.Serializable]
public class ShotPattern
{
    public const int GridWidth = 5; // Constant width
    public const int GridHeight = 5; // Constant height

    [SerializeField, HideInInspector]
    private int[] _grid = new int[GridWidth * GridHeight]; // 5x5 grid

    // Public properties to access grid dimensions
    public int Width => GridWidth;
    public int Height => GridHeight;
    public int TotalCells => _grid.Length;

    // 2D indexer with bounds checking
    public int this[int x, int y]
    {
        get => IsValidIndex(x, y) ? _grid[y * GridWidth + x] : 0;
        set
        {
            if (IsValidIndex(x, y))
                _grid[y * GridWidth + x] = Mathf.Max(0, value);
        }
    }

    private bool IsValidIndex(int x, int y)
        => x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;

    public void ResetGrid(int value = 0)
    {
        for (int i = 0; i < _grid.Length; i++)
            _grid[i] = value;
    }

    // New method to check if coordinates are within bounds
    public bool IsInBounds(int x, int y) => IsValidIndex(x, y);
}