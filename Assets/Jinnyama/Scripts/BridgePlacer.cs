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

    [Header("橋の高さ")]
    [Tooltip("GridMap.floorY からの相対オフセット。プレハブのpivotが板の中心なので、板厚の半分だけ上げると下端が床と揃う")]
    [SerializeField] float bridgeYOffset =0f;

    [Header("サウンド")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip placeSE;
    [SerializeField] AudioClip removeSE;

    readonly Dictionary<Vector2Int, GameObject> placed = new();

    [Header("クリック判定のゆるさ")]
    [Tooltip("プレイヤーとの距離判定の余裕（マス単位）")]
    [SerializeField] float nearMargin = 0.4f;
    [Tooltip("クリック位置からこの距離（マス単位）以内に置ける穴があれば、そちらに吸着する")]
    [SerializeField] float snapDistance = 0.8f;

    Vector3 lastClickPoint;


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
            TryPlace(PickPlaceableCell(mouse));
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

        var plane = new Plane(Vector3.up, new Vector3(0f, map.floorY, 0f));

        if(!plane.Raycast(ray, out float enter))
        {
            Debug.Log("[BridgePlacer]床平面と交差しませんでした");
            return new Vector2Int(int.MinValue, int.MinValue); // 必ず InBounds=false になる値
        }

        lastClickPoint = ray.GetPoint(enter);
        Vector2Int c = map.WorldToCell(lastClickPoint);
        Debug.Log($"[BridgePlacer] クリック位置 → セル {c}");
        return c;
    }

    // クリックしたマスに置けない場合、近くの「置ける穴」に吸着させる
    Vector2Int PickPlaceableCell(Mouse mouse)
    {
        Vector2Int c = GetCell(mouse);
        if (c.x == int.MinValue) return c;
        if (map.CanPlaceBridge(c) && IsNearPlayer(c)) return c;

        Vector2Int best = c;
        float bestDist = float.MaxValue;
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            var n = new Vector2Int(c.x + dx, c.y + dy);
            if (!map.CanPlaceBridge(n) || !IsNearPlayer(n)) continue;
            Vector3 w = map.CellToWorld(n);
            float d = Mathf.Max(Mathf.Abs(w.x - lastClickPoint.x), Mathf.Abs(w.z - lastClickPoint.z)) / map.cellSize;
            if (d < bestDist) { bestDist = d; best = n; }
        }
        if (bestDist <= snapDistance)
        {
            Debug.Log($"[BridgePlacer] {c} には置けないので近くの穴 {best} に吸着");
            return best;
        }
        return c;
    }


    bool IsNearPlayer(Vector2Int c)
    {
        if (player == null) return true; // プレイヤーが未設定なら制限なし

        // プレイヤーは自由移動なので、マスに丸めず実座標で判定する
        // （マスの端に立っていても隣の穴に置けるよう、プレイヤーの半径分だけ余裕を持たせる）
        Vector3 cw = map.CellToWorld(c);
        float dx = Mathf.Abs(player.position.x - cw.x) / map.cellSize;
        float dz = Mathf.Abs(player.position.z - cw.z) / map.cellSize;
        float limit = placeableRadius + 0.5f + nearMargin;
        return dx <= limit && dz <= limit;
    }

    void TryPlace(Vector2Int c)
    {
        if (stock <= 0)           { Debug.Log($"[BridgePlacer] 在庫切れ (stock={stock})"); return; }
        if (!map.CanPlaceBridge(c)) { Debug.Log($"[BridgePlacer] CanPlaceBridge({c}) = false（穴マスでない等）"); return; }
        if (!IsNearPlayer(c))     { Debug.Log($"[BridgePlacer] プレイヤーから遠すぎる ({c}) プレイヤー位置={player.position}"); return; }

        Debug.Log($"[BridgePlacer] 橋を設置: {c}");
        Vector3 pos = map.CellToWorld(c) + Vector3.up * bridgeYOffset;
        placed[c] = Instantiate(bridgePrefab, pos, Quaternion.identity);
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