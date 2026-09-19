using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    // シーン内のすべてのコンベアーを一括管理するリスト
    public static List<ConveyorBelt> activeBelts = new List<ConveyorBelt>();

    [Header("コンベアー設定")]
    [Tooltip("流す速度")]
    [SerializeField] private float speed = 2.5f;

    [Tooltip("初期の流れる向き（デフォルトは西向き/画面左向き）")]
    [SerializeField] private Vector3 baseDirection = Vector3.left;

    [Tooltip("現在反転中かどうか")]
    public bool isReversed = false;

    private void OnEnable()
    {
        activeBelts.Add(this);
    }

    private void OnDisable()
    {
        activeBelts.Remove(this);
    }

    private void Start()
    {
        // 乗ったキャラを検知するため、床の少し上に検知エリア（Trigger）を自動作成
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0.6f, 0f);
        trigger.size = new Vector3(0.95f, 0.4f, 0.95f);
    }

    private void OnTriggerStay(Collider other)
    {
        // 現在の流れる向きを計算
        Vector3 currentDir = isReversed ? -baseDirection : baseDirection;
        Vector3 movement = currentDir * speed * Time.deltaTime;

        // 乗っているオブジェクトの移動方式に合わせて位置を動かす
        if (other.TryGetComponent<CharacterController>(out var cc))
        {
            cc.Move(movement);
        }
        else if (other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.MovePosition(rb.position + movement);
        }
        else
        {
            other.transform.position += movement;
        }
    }

    // 全コンベアーの向きを一括で反転させる関数（レバーから呼ぶ）
    public static void ToggleAllBelts()
    {
        foreach (var belt in activeBelts)
        {
            if (belt != null)
            {
                belt.isReversed = !belt.isReversed;
            }
        }
    }
}