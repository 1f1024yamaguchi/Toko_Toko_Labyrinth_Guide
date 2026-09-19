using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BridgePlacer : MonoBehaviour
{
    [SerializeField] GridMap map;
    [SerializeField] GameObject bridgePrefab;
    [SerializeField] int stock = 3;
    [SerializeField] Camera cam;

    [SerializeField] TMP_Text stockText;
    [SerializeField] Transform player;            // 実際のプレイヤーオブジェクトの参照
    [SerializeField] int placeableRadius = 1;     // 橋を置ける周囲の距離（1なら周囲8マス）

    readonly Dictionary<Vector2Int, GameObject> placed = new();

    void Start()
    {
        UpdateStockText();
    }

    void UpdateStockText()
    {
        stockText.text = stock.ToString(); 
    }

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

    bool IsNearPlayer(Vector2Int c)
    {
        if (player == null) return true; // プレイヤーが未設定なら制限なし
        
        // プレイヤーのTransform座標から現在のグリッド上のマスを計算
        Vector2Int pCell = map.WorldToCell(player.position);
        
        // プレイヤーの現在位置とクリックした位置の差が、縦横ともに設定した範囲内か判定
        return Mathf.Abs(c.x - pCell.x) <= placeableRadius && 
               Mathf.Abs(c.y - pCell.y) <= placeableRadius;
    }

    void TryPlace(Vector2Int c)
    {
        // 橋が置けない条件、またはプレイヤーから離れすぎている場合は置かない
        if (stock <= 0 || !map.CanPlaceBridge(c) || !IsNearPlayer(c)) return;
        placed[c] = Instantiate(bridgePrefab, map.CellToWorld(c), Quaternion.identity);
        stock--;
        map.SetBridge(c, true);    // 見た目を置いてからデータを更新する
        UpdateStockText();
    }

    void TryRemove(Vector2Int c)   // 撤去して本数を回収する
    {
        if (!placed.TryGetValue(c, out var go)) return;
        Destroy(go);
        placed.Remove(c);
        stock++;
        map.SetBridge(c, false);
        UpdateStockText();
    }
}