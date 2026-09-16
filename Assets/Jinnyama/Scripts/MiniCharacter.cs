using UnityEngine;
using UnityEngine.InputSystem;
public class MiniCharacter : MonoBehaviour
{
    enum State { Idle, Moving, Waiting, Fallen, Goal }

    [SerializeField] GridMap map;
    [SerializeField] FlowField flow;
    [SerializeField] float speed = 2f;

    State state = State.Idle;
    Vector2Int cell, target;
    bool stopRequested;

    void Start()
    {
        cell = map.WorldToCell(transform.position);
        transform.position = map.CellToWorld(cell);
        flow.OnUpdated += OnFlowUpdated;
    }
    void OnDestroy() => flow.OnUpdated -= OnFlowUpdated;

    // 動き始め（スタートボタンやリーダーの号令から呼ぶ）
    public void Go()
    {
        stopRequested = false;
        if (state == State.Idle || state == State.Waiting) DecideNext();
    }

    // ストップ：マスの途中では止めず、次のマスに着いたら止まる
    public void Stop()
    {
        if (state == State.Moving) stopRequested = true;
        else if (state == State.Waiting) state = State.Idle;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                Go();
            }
            if (keyboard.xKey.wasPressedThisFrame)
            {
                Stop();
            }
        }
        if (state != State.Moving) return;
        Vector3 dest = map.CellToWorld(target);
        transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
        if ((transform.position - dest).sqrMagnitude < 0.0001f)
        {
            cell = target;
            OnArrive();
        }
    }

    void OnArrive()
    {
        if (map.IsDeadly(cell)) { state = State.Fallen; /* 落下アニメなど */ return; }
        if (map.IsGoal(cell))   { state = State.Goal;   return; }
        if (stopRequested)      { stopRequested = false; state = State.Idle; return; }
        DecideNext();   // マスに着くたびに最新の経路を見直す
    }

    void DecideNext()
    {
        if (flow.TryGetNext(cell, out target))
        {
            state = State.Moving;
            transform.forward = map.CellToWorld(target) - map.CellToWorld(cell);
        }
        else state = State.Waiting;   // 道がない → その場で待つ
    }

    // 橋が置かれた → 待っていたキャラが再び動き出す
    void OnFlowUpdated()
    {
        if (state == State.Waiting) DecideNext();
    }
}