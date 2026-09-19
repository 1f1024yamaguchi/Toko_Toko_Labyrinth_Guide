using UnityEngine;

public class StageTile : MonoBehaviour
{
    public TileType type = TileType.Floor;   // インスペクターでプルダウン選択

    // シーンビューで種類が分かるように色を表示
    void OnDrawGizmos()
    {
        switch (type)
        {
            case TileType.Wall:  Gizmos.color = Color.gray;   break;
            case TileType.Hole:  Gizmos.color = Color.black;  break;
            case TileType.Spike: Gizmos.color = Color.red;    break;
            case TileType.Goal:  Gizmos.color = Color.yellow; break;
            default: return;
        }
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.1f, 1f));
    }
}