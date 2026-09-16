using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    [SerializeField] GridMap map;
    [SerializeField] Vector2Int goal;
    int[,] dist;   // -1 = たどり着けない, 0 = ゴール
    public event Action OnUpdated;

    static readonly Vector2Int[] Dirs =
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    void OnEnable()  => map.OnChanged += Rebuild;
    void OnDisable() => map.OnChanged -= Rebuild;
    void Start() => Rebuild();

    public void Rebuild()
    {
        dist = new int[map.width, map.height];
        for (int x = 0; x < map.width; x++)
            for (int y = 0; y < map.height; y++) dist[x, y] = -1;

        var q = new Queue<Vector2Int>();
        dist[goal.x, goal.y] = 0;
        q.Enqueue(goal);

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            foreach (var d in Dirs)
            {
                var n = c + d;
                if (!map.IsWalkable(n) || dist[n.x, n.y] != -1) continue;
                dist[n.x, n.y] = dist[c.x, c.y] + 1;
                q.Enqueue(n);
            }
        }
        OnUpdated?.Invoke();
    }

    // 次に進むマスを返す。道がない・すでにゴールなら false
    public bool TryGetNext(Vector2Int from, out Vector2Int next)
    {
        next = from;
        if (!map.InBounds(from)) return false;
        int best = dist[from.x, from.y];
        if (best <= 0) return false;

        bool found = false;
        foreach (var d in Dirs)
        {
            var n = from + d;
            if (!map.InBounds(n)) continue;
            int dn = dist[n.x, n.y];
            if (dn >= 0 && dn < best) { best = dn; next = n; found = true; }
        }
        return found;
    }
}