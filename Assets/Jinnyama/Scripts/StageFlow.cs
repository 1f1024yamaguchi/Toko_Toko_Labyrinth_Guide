using System.Collections;
using UnityEngine;

// プレイヤーのゴール → 仲間をゴールへ → 集計 → リザルト
public class StageFlow : MonoBehaviour
{
    [SerializeField] GridMap map;
    [SerializeField] Transform player;
    [SerializeField] ScoreManager score;
    [SerializeField] ResultUI resultUI;

    [Tooltip("ゴール後に止めるスクリプト（プレイヤー移動、BridgePlacer、MiniCommander など）")]
    [SerializeField] MonoBehaviour[] disableOnGoal;

    [Tooltip("ゴールしたら消すオブジェクト（主人公）")]
    [SerializeField] GameObject hideOnGoal;

    [SerializeField] bool requirePathToPlayer = true;  // ゴール時に道が切れていた仲間は取り残し
    [SerializeField] float maxWaitSeconds = 10f;       // 仲間を待つ最大時間
    [SerializeField] float resultDelay = 0.5f;

    bool finished;
    public bool IsFinished => finished;

    void Update()
    {
        if (finished || player == null) return;

        // プレイヤーがゴールのマスに入った
        Vector2Int c = map.WorldToCell(player.position);
        if (map.InBounds(c) && map.IsGoal(c))
        {
            StartCoroutine(FinishRoutine());
            return;
        }

        // 全員取り残しになったら、すぐに失敗リザルト
        if (MiniCharacter.All.Count > 0 && AllLost())
        {
            finished = true;
            SetControls(false);
            score.SetResult(0, MiniCharacter.All.Count);
            resultUI.Show(0, MiniCharacter.All.Count);
        }
    }

    IEnumerator FinishRoutine()
    {
        finished = true;
        SetControls(false);
        if (hideOnGoal != null) hideOnGoal.SetActive(false);   // 主人公を消す

        MiniCharacter[] minis = MiniCharacter.All.ToArray();
        foreach (var m in minis) m.GoToGoal(requirePathToPlayer);

        // 全員の結果が出るまで待つ
        float t = 0f;
        while (t < maxWaitSeconds && !AllResolved(minis))
        {
            t += Time.deltaTime;
            yield return null;
        }

        int goaled = 0;
        foreach (var m in minis)
            if (m.CurrentTeam == MiniCharacter.Team.Goaled) goaled++;

        score.SetResult(goaled, minis.Length);
        yield return new WaitForSeconds(resultDelay);
        resultUI.Show(goaled, minis.Length);
    }

    void SetControls(bool on)
    {
        foreach (var b in disableOnGoal)
            if (b != null) b.enabled = on;
    }

    static bool AllResolved(MiniCharacter[] minis)
    {
        foreach (var m in minis) if (!m.IsResolved) return false;
        return true;
    }

    static bool AllLost()
    {
        foreach (var m in MiniCharacter.All)
            if (m.CurrentTeam != MiniCharacter.Team.Lost) return false;
        return true;
    }
}