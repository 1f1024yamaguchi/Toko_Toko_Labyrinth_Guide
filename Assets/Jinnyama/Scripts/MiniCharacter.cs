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

    [Header("ぶつからないようにする")]
    [Tooltip("マス1個分を1.0とした距離。ほかのミニキャラがこれより近いと離れる")]
    [SerializeField] float separationRadius = 0.8f;
    [Tooltip("マス1個分を1.0とした距離。プレイヤーがこれより近いと離れる")]
    [SerializeField] float playerSeparationRadius = 0.8f;
    [Tooltip("離れるときの速さ")]
    [SerializeField] float separationSpeed = 1.5f;
    [Tooltip("ミニキャラ同士は押し合わずにすり抜ける（詰まって動けなくなるのを防ぐ）")]
    [SerializeField] bool passThroughOtherMinis = true;

    [Header("見た目")]
    [SerializeField] ParticleSystem followFx;
    [SerializeField] ParticleSystem joinFx;

    Rigidbody rb;
    Transform player;
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

        var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) player = playerObj.transform;

        SetupPassThrough();

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
        if (team == Team.Stopped) 
        { 
            team = Team.Follow; 
            SetFollowFx(true); 
        }

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

        Vector3 sep = SeparationVelocity();   // 近すぎる相手から離れる分

        if (move != Move.Moving)
        {
                        // 止まっているときでも、近すぎるなら退いて場所をあける
            rb.linearVelocity = new Vector3(sep.x, rb.linearVelocity.y, sep.z);
            SyncCell();
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
        Vector3 v = dir * step + sep;
        rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
        transform.forward = dir;
    }

    // ---------- お互いに距離をとる ----------

    // 近すぎる相手（ほかのミニキャラ・プレイヤー）から離れる速度を作る
    Vector3 SeparationVelocity()
    {
        if (separationSpeed <= 0f) return Vector3.zero;

        Vector3 away = Vector3.zero;

        for (int i = 0; i < All.Count; i++)
        {
            var other = All[i];
            if (other == null || other == this || other.IsResolved) continue;
            away += AwayFrom(other.transform.position, separationRadius);
        }

                // 仲間になったミニキャラだけプレイヤーから離れる
        // ・Wild / Stopped は触って仲間にしたいので逃げない
        // ・プレイヤーがゴールして非表示になったあとは、その場に Transform だけ残る。
        //   それから逃げるとゴールのマスに入れなくなるので、非表示のときは無視する
        if (player != null && player.gameObject.activeInHierarchy && team == Team.Follow)
            away += AwayFrom(player.position, playerSeparationRadius);

        if (away.sqrMagnitude < 0.000001f) return Vector3.zero;

        return AllowedMove(Vector3.ClampMagnitude(away, 1f) * separationSpeed);
    }

    // 相手から離れる向き。近いほど強くなる
    Vector3 AwayFrom(Vector3 otherPos, float radiusInCells)
    {
        float radius = radiusInCells * map.cellSize;
        if (radius <= 0f) return Vector3.zero;

        Vector3 d = transform.position - otherPos;
        d.y = 0f;
        float dist = d.magnitude;
        if (dist >= radius) return Vector3.zero;

        if (dist < 0.001f)   // ほぼ重なっている → 個体ごとに決まった向きへ逃がす
        {
            float a = (Mathf.Abs(GetInstanceID()) % 360) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
        }

        return d / dist * (1f - dist / radius);
    }

    // 穴・とげ・壁のほうへは押し出さない
    Vector3 AllowedMove(Vector3 v)
    {
        if (v.sqrMagnitude < 0.000001f) return Vector3.zero;

        float look = Time.fixedDeltaTime * 4f;   // 少し先を見て確かめる
        if (map.IsWalkable(map.WorldToCell(transform.position + v * look))) return v;

        Vector3 vx = new Vector3(v.x, 0f, 0f);
        if (map.IsWalkable(map.WorldToCell(transform.position + vx * look))) return vx;

        Vector3 vz = new Vector3(0f, 0f, v.z);
        if (map.IsWalkable(map.WorldToCell(transform.position + vz * look))) return vz;

        return Vector3.zero;
    }

    // 押されて別のマスに移ったときは、今いるマスを取り直して行き先を考え直す
    void SyncCell()
    {
        Vector2Int c = map.WorldToCell(transform.position);
        if (c == cell || !map.IsWalkable(c)) return;

        cell = c;
        if (team == Team.Follow || team == Team.ToGoal) DecideNext();
    }

    // ミニキャラ同士の当たり判定を切る（押し合いで進行不能にならないように）
    void SetupPassThrough()
    {
        if (!passThroughOtherMinis) return;

        var mine = GetComponentsInChildren<Collider>();

        for (int i = 0; i < All.Count; i++)
        {
            var other = All[i];
            if (other == null || other == this) continue;

            foreach (var oc in other.GetComponentsInChildren<Collider>())
            {
                if (oc == null) continue;
                foreach (var mc in mine)
                {
                    if (mc == null) continue;
                    Physics.IgnoreCollision(mc, oc, true);
                }
            }
        }
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
        Destroy(gameObject); 
    }

    void SetFollowFx(bool on)
    {
        if (followFx == null) return;
        if (on) 
        {
            followFx.Play();
        }
        else
        {
            followFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}