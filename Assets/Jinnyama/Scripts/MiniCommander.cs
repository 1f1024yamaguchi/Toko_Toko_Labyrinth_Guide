using UnityEngine;
using UnityEngine.InputSystem;

public class MiniCommander : MonoBehaviour
{
    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.spaceKey.wasPressedThisFrame)
            foreach (var m in FindObjectsByType<MiniCharacter>(FindObjectsSortMode.None)) m.Go();
        if (kb.sKey.wasPressedThisFrame)
            foreach (var m in FindObjectsByType<MiniCharacter>(FindObjectsSortMode.None)) m.Stop();
    }
}