using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BridgePlacer : MonoBehaviour
{
    [SerializeField] GridMap map;
    [SerializeField] GameObject bridgePrefab;
    [SerializeField] int stock = 3;
    [SerializeField] Camera cam;
    readonly Dictionary<Vector2Int, GameObject> placed = new();

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;   // マウスがつながっていないとき

        if (mouse.leftButton.wasPressedThisFrame)  TryPlace(GetCell(mouse));
        if (mouse.rightButton.wasPressedThisFrame) TryRemove(GetCell(mouse));
    }

    Vector2Int GetCell(Mouse mouse)
    {
        var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float enter);
        return map.WorldToCell(ray.GetPoint(enter));
    }

    void TryPlace(Vector2Int c)
    {
        if (stock <= 0 || !map.CanPlaceBridge(c)) return;
        placed[c] = Instantiate(bridgePrefab, map.CellToWorld(c), Quaternion.identity);
        stock--;
        map.SetBridge(c, true);    // 見た目を置いてからデータを更新する
    }

    void TryRemove(Vector2Int c)   // 撤去して本数を回収する
    {
        if (!placed.TryGetValue(c, out var go)) return;
        Destroy(go);
        placed.Remove(c);
        stock++;
        map.SetBridge(c, false);
    }
}