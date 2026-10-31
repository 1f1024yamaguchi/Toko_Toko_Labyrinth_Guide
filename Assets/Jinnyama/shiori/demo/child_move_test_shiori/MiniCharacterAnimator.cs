using UnityEngine;

// MiniCharacter の Rigidbody の速度を見て、子のモデルの Animator を動かす
// TestPlayerController と同じパラメータ名（Speed / VSpeed / IsGrounded）を使うので、
// 動作確認で作った Animator Controller をそのまま使える
[RequireComponent(typeof(Rigidbody))]
public class MiniCharacterAnimator : MonoBehaviour
{
    [Header("参照(空欄なら子から自動取得)")]
    [SerializeField] Animator animator;

    [Header("調整")]
    [Tooltip("Speed の変化をなめらかにする時間。押し合いで歩き/待機がチラつくのを防ぐ")]
    [SerializeField] float speedDampTime = 0.1f;
    [Tooltip("これより速く下に落ちていたら空中扱い")]
    [SerializeField] float fallVelocityThreshold = 0.5f;

    Rigidbody rb;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int VerticalSpeedHash = Animator.StringToHash("VSpeed");
    static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        Vector3 v = rb.linearVelocity;
        float horizontalSpeed = new Vector2(v.x, v.z).magnitude;

        animator.SetFloat(SpeedHash, horizontalSpeed, speedDampTime, Time.deltaTime);
        animator.SetFloat(VerticalSpeedHash, v.y);
        // ミニキャラはジャンプしないので、下向きに速く動いているときだけ空中（落下中）とみなす
        animator.SetBool(IsGroundedHash, v.y > -fallVelocityThreshold);
    }
}
