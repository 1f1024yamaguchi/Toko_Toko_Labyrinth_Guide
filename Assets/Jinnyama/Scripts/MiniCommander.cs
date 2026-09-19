using UnityEngine;
using UnityEngine.InputSystem;

public class MiniCommander : MonoBehaviour
{
    [SerializeField] InputActionReference GO;
    [SerializeField] InputActionReference STOP;

    void OnEnable()
    {
        if (GO != null) GO.action.Enable();
        if (STOP != null) STOP.action.Enable();
    }

    void OnDisable()
    {
        if (GO != null) GO.action.Disable();
        if (STOP != null) STOP.action.Disable();
    }

    void Update()
    {
        // Goアクションが押された瞬間
        if (GO != null && GO.action.WasPressedThisFrame())
        {
            foreach (var m in FindObjectsByType<MiniCharacter>(FindObjectsSortMode.None)) m.Go();
        }

        // Stopアクションが押された瞬間
        if (STOP != null && STOP.action.WasPressedThisFrame())
        {
            foreach (var m in FindObjectsByType<MiniCharacter>(FindObjectsSortMode.None)) m.Stop();
        }
    }
}