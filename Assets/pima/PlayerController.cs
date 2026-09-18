using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("キャラクターの移動速度")]
    public float moveSpeed = 5f;

    [Tooltip("キャラクターが振り向く速さ")]
    public float rotationSpeed = 10f;

    [Header("参照")]
    [Tooltip("基準にするカメラ（空欄の場合は自動でメインカメラを探します）")]
    public Transform cameraTransform;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // もしカメラが手動で設定されていなければ、シーン内のメインカメラを自動取得する
        if (cameraTransform == null)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("シーン内にメインカメラが見つかりません！");
            }
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // --- 1. キーボード入力の取得 ---
        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;

        // 入力ベクトルを作成して長さを整える
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // キーが押されている場合のみ移動・回転処理を行う
        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 moveDirection = Vector3.zero;

            // --- 2. カメラ基準の移動方向を計算 ---
            if (cameraTransform != null)
            {
                // カメラの前方向と右方向を取得
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;

                // 上下の傾き（Y成分）をゼロにして水平にする
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                // カメラの前方向×前後入力 + カメラの右方向×左右入力
                moveDirection = (forward * inputDirection.z + right * inputDirection.x).normalized;
            }
            else
            {
                // カメラが見つからない場合の保険（従来のワールド基準移動）
                moveDirection = inputDirection;
            }

            // --- 3. 進行方向を向く（旋回） ---
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // --- 4. 移動する ---
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // --- 5. 重力処理 ---
        Vector3 gravity = new Vector3(0, -9.81f * Time.deltaTime, 0);
        controller.Move(gravity);
    }
}