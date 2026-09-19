using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class Next_Stage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     //Invoke("ChangeScane",1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public SceneAsset LoadtoScene;
   public void ChangeScene()
    {
        SceneManager.LoadScene(LoadtoScene.name);
    }
}
