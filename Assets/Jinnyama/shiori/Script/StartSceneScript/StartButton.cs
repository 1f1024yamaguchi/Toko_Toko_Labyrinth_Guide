using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    // Startボタンを押す
    public void ChangeScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    }
}
