using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MiniCharacter : MonoBehaviour
{
    public enum Team { Wild, Follow, Stopped, ToGoal, Goaled, Lost }
    enum Move { Idle, Moving, Waiting }

    public static readonly List<MiniCharacter> All = new();

    [Header("参照")]
    [SerializeField] GridMap map;
    [SerializeField] FlowField playerFlow;
    [SerializeField] FlowField goalFlow;

    [Header("動き")]
    [SerializeField] float speed = 2f;
    [SerializeField] int keepDistance = 1;
    [SerializeField] float arriveDistance = 0.05f;   // このくらい近づいたら到着
    [SerializeField] float fallLimit = 2f;           // 開始位置よりこれだけ落ちたら落下扱い
    [SerializeField] string playerTag = "Player";

    [Header("見た目")]
    [SerializeField] ParticleSystem followFx;
    [SerializeField] ParticleSystem joinFx;

    Rigidbody rb;
    Team team = Team.Wild;
    Move move = Move.Idle;
    Vector2Int cell, target;
    float startY;

    public Team CurrentTeam => team;
    public Vector2Int CurrentCell => cell;
    public bool IsResolved => team == Team.Goaled || team == Team.Lost;
    public event Action<MiniCharacter> OnResolved;

    void OnEnable()  => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Awake() => rb = GetComponent<Rigidbody>();

    void Start()
    {
        cell = map.WorldToCell(transform.position);
        startY = transform.position.y;              // 高さは今の位置を保つ
        Vector3 p = map.CellToWorld(cell);
        p.y = startY;
        transform.position = p;

        playerFlow.OnUpdated += OnFlowUpdated;
        goalFlow.OnUpdated += OnFlowUpdated;
        SetFollowFx(false);
    }

    void OnDestroy()
    {
        if (playerFlow != null) playerFlow.OnUpdated -= OnFlowUpdated;
        if (goalFlow != null) goalFlow.OnUpdated -= OnFlowUpdated;
    }

    // ---------- プレイヤーに触れた ----------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (team == Team.Wild) Join();
        else if (team == Team.Stopped) Go();
    }

    public void Join()
    {
        if (team != Team.Wild) return;
        team = Team.Follow;
        if (joinFx != null) joinFx.Play();
        SetFollowFx(true);
        DecideNext();
    }

    public void Go()
    {
        if (team == Team.Stopped) { team = Team.Follow; SetFollowFx(true); }
        if (team != Team.Follow) return;
        if (move != Move.Moving) DecideNext();
    }

    public void Stop()
    {
        if (team != Team.Follow) return;
        team = Team.Stopped;
        SetFollowFx(false);
        if (move != Move.Moving) move = Move.Idle;
    }

    // ---------- プレイヤーがゴールしたとき ----------
    public void GoToGoal(bool requirePathToPlayer)
    {
        if (IsResolved) return;

        bool ok = team == Team.Follow &&
                  (!requirePathToPlayer || playerFlow.HasPath(cell));
        if (!ok) { Resolve(Team.Lost); return; }

        team = Team.ToGoal;
        if (move != Move.Moving) DecideNext();
    }

    // ---------- 移動（速度で動かす） ----------
    void FixedUpdate()
    {
        if (IsResolved) return;

        // 穴などに落ちたとき
        if (transform.position.y < startY - fallLimit) { Fall(); return; }

        if (move != Move.Moving)
        {
            StopHorizontal();
            return;
        }

        Vector3 dest = map.CellToWorld(target);
        Vector3 d = dest - transform.position;
        d.y = 0f;
        float dist = d.magnitude;

        if (dist < arriveDistance)
        {
            StopHorizontal();
            cell = target;
            OnArrive();
            return;
        }

        Vector3 dir = d / dist;
        float step = Mathf.Min(speed, dist / Time.fixedDeltaTime);   // 行き過ぎ防止
        rb.linearVelocity = new Vector3(dir.x * step, rb.linearVelocity.y, dir.z * step);
        transform.forward = dir;
    }

    void StopHorizontal() =>
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

    void OnArrive()
    {
        if (map.IsDeadly(cell)) { Fall(); return; }
        if (team == Team.ToGoal && map.IsGoal(cell)) { Resolve(Team.Goaled); return; }
        if (team != Team.Follow && team != Team.ToGoal) { move = Move.Idle; return; }
        DecideNext();
    }

    void DecideNext()
    {
        if (team == Team.ToGoal)
        {
            if (map.IsGoal(cell)) { Resolve(Team.Goaled); return; }
            if (goalFlow.TryGetNext(cell, out target)) move = Move.Moving;
            else Resolve(Team.Lost);
            return;
        }

        if (team != Team.Follow) { move = Move.Idle; return; }

        int d = playerFlow.GetDistance(cell);
        if (d >= 0 && d <= keepDistance) { move = Move.Waiting; return; }

        if (playerFlow.TryGetNext(cell, out target)) move = Move.Moving;
        else move = Move.Waiting;   // 道がない → 待つ（follow は外れない）
    }

    void OnFlowUpdated()
    {
        if (IsResolved) return;
        if (move == Move.Waiting) DecideNext();
    }

    void Fall()
    {
        Resolve(Team.Lost);
    }

    void Resolve(Team result)
    {
        team = result;
        move = Move.Idle;
        SetFollowFx(false);
        OnResolved?.Invoke(this);
    }

    void SetFollowFx(bool on)
    {
        if (followFx == null) return;
        if (on && !followFx.isPlaying) followFx.Play();
        else if (!on) followFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}