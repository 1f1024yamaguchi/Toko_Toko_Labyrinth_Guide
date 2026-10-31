using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    public GameObject pauseCanvas;
    bool isPaused;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isPaused=!isPaused;
            pauseCanvas.SetActive(isPaused);
            if (isPaused)
            {
                Time.timeScale=0f;
            }
            else
            {
                Time.timeScale=1f;
            }
        }
    }
}
