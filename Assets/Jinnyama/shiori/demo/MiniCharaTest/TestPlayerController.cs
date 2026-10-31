using UnityEngine;

// 動作確認用の仮スクリプト
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class TestPlayerController : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("ジャンプ・重力")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("参照(空欄ならAwakeで自動取得)")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;

    private Vector3 verticalVelocity; // y成分だけ使う(重力・ジャンプ用)
    private bool isGrounded;
    private float currentHorizontalSpeed;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (controller == null) controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        UpdateGroundCheck();
        UpdateMoveAndRotate();
        UpdateJumpAndGravity();
        UpdateAnimatorParameters();
    }

    private void UpdateGroundCheck()
    {
        isGrounded = controller.isGrounded;

        // 接地中に重力が溜まり続けて次のジャンプで不自然に沈むのを防ぐ
        if (isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }
    }

    private void UpdateMoveAndRotate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDir = inputDir.normalized;
        Vector3 horizontalVelocity = moveDir * (targetSpeed * inputDir.magnitude);
        controller.Move(horizontalVelocity * Time.deltaTime);
        currentHorizontalSpeed = horizontalVelocity.magnitude;

        // 進行方向へ体を向ける(見た目確認用)
        if (inputDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateJumpAndGravity()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(JumpHash);
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat(SpeedHash, currentHorizontalSpeed);
        animator.SetFloat(VerticalSpeedHash, verticalVelocity.y);
        animator.SetBool(IsGroundedHash, isGrounded);
    }

    // ---- 画面左上に数値を表示するだけの簡易デバッグHUD ----
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 320, 22), $"Speed: {currentHorizontalSpeed:F2}");
        GUI.Label(new Rect(10, 30, 320, 22), $"VerticalSpeed: {verticalVelocity.y:F2}");
        GUI.Label(new Rect(10, 50, 320, 22), $"IsGrounded: {isGrounded}");
        GUI.Label(new Rect(10, 70, 320, 22), $"Clip: {GetCurrentClipName()}");
    }

    private string GetCurrentClipName()
    {
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        return clips.Length > 0 ? clips[0].clip.name : "-";
    }
}