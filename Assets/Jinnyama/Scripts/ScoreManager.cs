using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TMP_Text followText;   // 「ついてきている 5 / 8」

    public int Goaled { get; private set; }
    public int Total { get; private set; }

    void Update()
    {
        if (followText == null) return;
        int follow = 0;
        foreach (var m in MiniCharacter.All)
            if (m.CurrentTeam == MiniCharacter.Team.Follow) follow++;
        followText.text = $" {follow} / {MiniCharacter.All.Count}";
    }

    public void SetResult(int goaled, int total)
    {
        Goaled = goaled;
        Total = total;
    }
}