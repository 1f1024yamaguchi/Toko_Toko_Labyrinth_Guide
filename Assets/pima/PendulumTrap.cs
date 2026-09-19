using UnityEngine;

public class PendulumTrap : MonoBehaviour
{
    [Header("振り子の設定")]
    [Tooltip("最大で傾く角度（片側）")]
    [SerializeField] private float maxAngle = 60f;

    [Tooltip("揺れる速さ")]
    [SerializeField] private float speed = 2.0f;

    [Tooltip("揺れ始めるタイミングのズレ（複数配置してずらす時に便利）")]
    [SerializeField] private float timeOffset = 0f;

    [Header("スイング軸（ダンジョンの向きに合わせて選択）")]
    [SerializeField] private Vector3 swingAxis = Vector3.forward; // Z軸周り（左右に振る）

    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        // サイン波を使って -1.0 〜 +1.0 の間を滑らかに往復
        float angle = Mathf.Sin((Time.time + timeOffset) * speed) * maxAngle;

        // 支点を中心に回転させる
        transform.localRotation = initialRotation * Quaternion.AngleAxis(angle, swingAxis);
    }
}