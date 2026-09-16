using UnityEngine;

public class StageSetup : MonoBehaviour
{
    [SerializeField] GridMap map;
    [SerializeField] FlowField flow;
    [SerializeField] Transform tilesRoot;

    void Start()
    {
        // 1. まず全部のマスを「穴」にする
        for (int x = 0; x < map.width; x++)
            for (int y = 0; y < map.height; y++)
                map.SetTile(new Vector2Int(x, y), TileType.Hole);

        // 2. ブロックがあるマスだけ、その種類で上書きする
        StageTile[] tiles = tilesRoot != null
            ? tilesRoot.GetComponentsInChildren<StageTile>()
            : FindObjectsByType<StageTile>(FindObjectsSortMode.None);

        foreach (var tile in tiles)
        {
            Vector2Int c = map.WorldToCell(tile.transform.position);
            if (map.InBounds(c)) map.SetTile(c, tile.type);
        }
        flow.Rebuild();
    }
}