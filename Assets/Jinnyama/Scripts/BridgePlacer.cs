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

    [Header("サウンド")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip placeSE;
    [SerializeField] AudioClip removeSE;

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
        if (mouse == null) { Debug.Log("[BridgePlacer] Mouse.current が null"); return; }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[BridgePlacer] 左クリック検出");
            TryPlace(GetCell(mouse));
        }
        if (mouse.rightButton.wasPressedThisFrame)
        {
            Debug.Log("[BridgePlacer] 右クリック検出");
            TryRemove(GetCell(mouse));
        }
    }

    Vector2Int GetCell(Mouse mouse)
    {
        if (cam == null) { Debug.LogError("[BridgePlacer] cam が null!"); return Vector2Int.zero; }
        if (map == null) { Debug.LogError("[BridgePlacer] map が null!"); return Vector2Int.zero; }
        var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float enter);
        Vector2Int c = map.WorldToCell(ray.GetPoint(enter));
        Debug.Log($"[BridgePlacer] クリック位置 → セル {c}");
        return c;
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
        if (stock <= 0)           { Debug.Log($"[BridgePlacer] 在庫切れ (stock={stock})"); return; }
        if (!map.CanPlaceBridge(c)) { Debug.Log($"[BridgePlacer] CanPlaceBridge({c}) = false（穴マスでない等）"); return; }
        if (!IsNearPlayer(c))     { Debug.Log($"[BridgePlacer] プレイヤーから遠すぎる ({c})"); return; }

        Debug.Log($"[BridgePlacer] 橋を設置: {c}");
        placed[c] = Instantiate(bridgePrefab, map.CellToWorld(c), Quaternion.identity);
        stock--;
        map.SetBridge(c, true);
        UpdateStockText();
        if (audioSource != null && placeSE != null) audioSource.PlayOneShot(placeSE);
    }

    void TryRemove(Vector2Int c)   // 撤去して本数を回収する
    {
        if (!placed.TryGetValue(c, out var go)) return;
        Destroy(go);
        placed.Remove(c);
        stock++;
        map.SetBridge(c, false);
        UpdateStockText();
        if (audioSource != null && removeSE != null) audioSource.PlayOneShot(removeSE);
    }
}