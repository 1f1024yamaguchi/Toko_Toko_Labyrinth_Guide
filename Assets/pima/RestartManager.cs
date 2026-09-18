using UnityEngine;
using UnityEngine.SceneManagement; // シーンの切り替え・再読み込みに必要な道具箱

public class RestartManager : MonoBehaviour
{
    /// <summary>
    /// 現在開いているステージ（シーン）を最初からやり直す関数
    /// </summary>
    public void RestartCurrentScene()
    {
        // 今遊んでいるシーンの名前を取得する
        string currentSceneName = SceneManager.GetActiveScene().name;

        // そのシーンをもう一度読み込み直す（完全初期化）
        SceneManager.LoadScene(currentSceneName);

        Debug.Log("ステージをリスタートしました: " + currentSceneName);
    }
}