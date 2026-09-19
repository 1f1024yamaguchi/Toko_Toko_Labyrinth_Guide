using UnityEngine;

public class SwitchLever : MonoBehaviour
{
    [Header("レバーの見た目設定")]
    [Tooltip("傾ける対象の棒オブジェクト（未設定なら自身を傾けます）")]
    [SerializeField] private Transform stickTransform;

    [Tooltip("傾く角度")]
    [SerializeField] private float tiltAngle = 30f;

    private bool isOn = false;
    private float lastToggleTime = 0f;
    private const float Cooldown = 0.5f; // 連続作動を防ぐ時間

    private void Start()
    {
        if (stickTransform == null)
        {
            stickTransform = transform;
        }

        // 触れたことを検知するためのTriggerを自動設定
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.2f, 1.2f, 1.2f);
        }

        UpdateLeverVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーや仲間が触れたか判定（連続接触はクールダウン）
        if (Time.time - lastToggleTime < Cooldown) return;

        // レバーの状態を切り替え
        isOn = !isOn;
        lastToggleTime = Time.time;

        // 見た目の傾きを更新
        UpdateLeverVisual();

        // 全ベルトコンベアーを反転！
        ConveyorBelt.ToggleAllBelts();

        Debug.Log($"レバー作動！ コンベアーの向きが反転しました（状態: {(isOn ? "ON" : "OFF")}）");
    }

    private void UpdateLeverVisual()
    {
        if (stickTransform == null) return;

        // Z軸周りにクイッと倒す演出
        float zRot = isOn ? tiltAngle : -tiltAngle;
        stickTransform.localRotation = Quaternion.Euler(0f, 0f, zRot);
    }
}