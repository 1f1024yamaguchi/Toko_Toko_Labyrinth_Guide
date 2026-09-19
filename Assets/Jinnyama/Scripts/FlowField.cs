using System;
using System.Collections.Generic;
using UnityEngine;

// target までの距離マップ。プレイヤー用とゴール用で2つ置いて使う
public class FlowField : MonoBehaviour
{
    [SerializeField] GridMap map;
    [SerializeField] Transform target;          // プレイヤー用ならプレイヤー、ゴール用ならゴール
    [SerializeField] bool trackTarget = false;  // プレイヤー用は ON（別のマスに動いたら再計算）

    int[,] dist;   // -1 = たどり着けない, 0 = target のマス
    Vector2Int lastTargetCell = new Vector2Int(int.MinValue, int.MinValue);
    public event Action OnUpdated;

    static readonly Vector2Int[] Dirs =
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    void OnEnable()  => map.OnChanged += Rebuild;
    void OnDisable() => map.OnChanged -= Rebuild;
    void Start() => Rebuild();

    void Update()
    {
        if(!trackTarget || target == null)
        {
            return;
        }

        if(map.WorldToCell(target.position) != lastTargetCell)
        {
            Rebuild();
        }
    }

    public void Rebuild()
    {
        dist = new int[map.width, map.height];
        for (int x = 0; x < map.width; x++)
            for (int y = 0; y < map.height; y++) dist[x, y] = -1;

        if (target != null)
        {
            Vector2Int t = map.WorldToCell(target.position);
            lastTargetCell = t;

            if (map.InBounds(t))
            {
                var q = new Queue<Vector2Int>();
                dist[t.x, t.y] = 0;
                q.Enqueue(t);

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
            }
        }
        OnUpdated?.Invoke();
    }

    // target までの歩数（道がなければ -1）
    public int GetDistance(Vector2Int c) =>
        (dist != null && map.InBounds(c)) ? dist[c.x, c.y] : -1;

    //道があるか
    public bool HasPath(Vector2Int c) => GetDistance(c) >= 0;

    // 次に進むマスを返す。道がない・すでに target なら false
    public bool TryGetNext(Vector2Int from, out Vector2Int next)
    {
        next = from;
        int best = GetDistance(from);
        if (best <= 0) return false;

        bool found = false;
        foreach (var d in Dirs)
        {
            int dn = GetDistance(from + d);
            if (dn >= 0 && dn < best) { best = dn; next = from + d; found = true; }
        }
        return found;
    }
}