using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultUI : MonoBehaviour
{
    [SerializeField] GameObject panel;        // リザルトのパネル（最初は非表示）

    [SerializeField] TMP_Text countText;   // 「ミニキャラ 3/8」
    

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show(int goaled, int total)
    {
        if (panel != null) panel.SetActive(true);
        if (countText != null) countText.text = $"ミニキャラ {goaled}/{total}";
    }



}