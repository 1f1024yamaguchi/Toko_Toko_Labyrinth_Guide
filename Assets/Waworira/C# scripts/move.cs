using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class move : MonoBehaviour
{
    public float speed=0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            transform.Translate(Vector3.forward*speed);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            transform.Translate(Vector3.left*speed);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            transform.Translate(Vector3.back*speed);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            transform.Translate(Vector3.right*speed);
        }

    }
}
