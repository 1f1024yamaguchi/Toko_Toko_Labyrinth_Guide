using UnityEngine;
using UnityEngine.InputSystem; // 新しい Input System パッケージに対応

namespace Otai5
{
    /// <summary>
    /// Q/Eキーで90度ずつ4方向に滑らかに回転する見下ろしカメラコントローラー
    /// （New Input System 対応版）
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Position Settings")]
        [Tooltip("カメラが注視する対象（未設定の場合は原点(0,0,0)を中心に回転します）")]
        [SerializeField] private Transform target;

        [Tooltip("注視点からのオフセット（高さなど）")]
        [SerializeField] private Vector3 targetOffset = new Vector3(0, 1f, 0);

        [Tooltip("カメラと中心点との距離")]
        [SerializeField] private float distance = 20f;

        [Header("Angle & Speed Settings")]
        [Tooltip("見下ろす角度 (10〜80度)")]
        [Range(10f, 80f)]
        [SerializeField] private float pitchAngle = 45f;

        [Tooltip("回転の滑らかさ（大きいほど速く回転）")]
        [SerializeField] private float rotationSpeed = 8f;

        private int currentAngleIndex = 0; // 0: 0度, 1: 90度, 2: 180度, 3: 270度
        private float targetYAngle = 0f;

        private void Update()
        {
            // 新しい Input System からキー入力を取得
            if (Keyboard.current != null)
            {
                // Qキーで左に90度回転
                if (Keyboard.current.qKey.wasPressedThisFrame)
                {
                    Debug.Log("[CameraController] Qキーが押されました -> 左回転");
                    RotateCamera(-1);
                }
                // Eキーで右に90度回転
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Debug.Log("[CameraController] Eキーが押されました -> 右回転");
                    RotateCamera(1);
                }
            }
        }

        private void LateUpdate()
        {
            // 目標のY軸角度へ向けて滑らかに回転補間
            float currentYAngle = transform.eulerAngles.y;
            float smoothYAngle = Mathf.LerpAngle(currentYAngle, targetYAngle, Time.deltaTime * rotationSpeed);

            // 回転（クォータニオン）の作成
            Quaternion rotation = Quaternion.Euler(pitchAngle, smoothYAngle, 0f);

            // 注視点（中心位置）
            Vector3 centerPosition = (target != null) ? target.position + targetOffset : targetOffset;

            // カメラの位置と向きを適用
            transform.position = centerPosition - (rotation * Vector3.forward * distance);
            transform.rotation = rotation;
        }

        /// <summary>
        /// 方向を指定して90度回転
        /// </summary>
        /// <param name="direction">-1: 左回転 / 1: 右回転</param>
        public void RotateCamera(int direction)
        {
            currentAngleIndex = (currentAngleIndex + direction) % 4;
            if (currentAngleIndex < 0) currentAngleIndex += 4;
            
            targetYAngle = currentAngleIndex * 90f;
        }
    }
}
