using UnityEngine;
using UnityEngine.InputSystem; // 最新のInput Systemを使うための宣言

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 5f;

    [Header("振り向く速さ")]
    public float rotationSpeed = 10f;

    private CharacterController controller;

    void Start()
    {
        // CharacterControllerコンポーネントを取得
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // キーボードが接続されているか確認
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float horizontal = 0f;
        float vertical = 0f;

        // A/Dキー または 左右矢印キーで左右移動
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;

        // W/Sキー または 上下矢印キーで前後移動
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;

        // 斜め移動でも速度が一定になるよう正規化
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // キーが押されている場合のみ移動・方向転換
        if (direction.magnitude >= 0.1f)
        {
            // 進行方向へスムーズに回転
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // キャラクターを移動
            controller.Move(direction * moveSpeed * Time.deltaTime);
        }

        // 重力をかけて地面から浮かないようにする
        Vector3 gravity = new Vector3(0, -9.81f * Time.deltaTime, 0);
        controller.Move(gravity);
    }
}