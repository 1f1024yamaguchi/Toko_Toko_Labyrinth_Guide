using System;
using UnityEngine;

public enum TileType { Floor, Wall, Hole, Spike, Goal }

public class GridMap : MonoBehaviour
{
    [Header("グリッド")]
    public int width = 10, height = 10;
    public float cellSize = 1f;

    [Header("高さ")]
    [Tooltip("床のY座標。橋の生成の高さとクリック判定の平面に使われる")]
    public float floorY = 0f;

    TileType[,] tiles;   // Tilemapやシーン上の配置から初期化する
    bool[,] bridged;

    public event Action OnChanged;

    void Awake()
    {
        tiles = new TileType[width, height];
        bridged = new bool[width, height];
    }

    public bool InBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;
    TileType Get(Vector2Int c) => tiles[c.x, c.y];

    // 通れるマス：穴やとげでも、橋があれば通れる
    public bool IsWalkable(Vector2Int c)
    {
        if (!InBounds(c)) return false;
        var t = Get(c);
        if (t == TileType.Wall) return false;
        if (t == TileType.Hole || t == TileType.Spike) return bridged[c.x, c.y];
        return true;
    }

    public bool IsDeadly(Vector2Int c) =>
        (Get(c) == TileType.Hole || Get(c) == TileType.Spike) && !bridged[c.x, c.y];

    public bool IsGoal(Vector2Int c) => Get(c) == TileType.Goal;

    public bool CanPlaceBridge(Vector2Int c) =>
        InBounds(c) && (Get(c) == TileType.Hole || Get(c) == TileType.Spike) && !bridged[c.x, c.y];

    public void SetBridge(Vector2Int c, bool on)
    {
        bridged[c.x, c.y] = on;
        OnChanged?.Invoke();   // ← ここで経路の再計算が走る
    }

    public void SetTile(Vector2Int c, TileType t) => tiles[c.x, c.y] = t;

    public Vector3 CellToWorld(Vector2Int c) => new Vector3(c.x * cellSize, floorY, c.y * cellSize);
    public Vector2Int WorldToCell(Vector3 p) =>
        new Vector2Int(Mathf.RoundToInt(p.x / cellSize), Mathf.RoundToInt(p.z / cellSize));
}