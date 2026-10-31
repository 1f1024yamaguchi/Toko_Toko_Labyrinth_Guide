using UnityEngine;

public class Destroy : MonoBehaviour
{
    public GameObject objecttobreak;
    private Renderer rend;
    public Color normalColor=Color.red;
    public Color activeColor=Color.green;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend=GetComponent<Renderer>();
        rend.material.color=normalColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            objecttobreak.SetActive(false);//消す
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            objecttobreak.SetActive(true);//元に戻す
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            rend.material.color=activeColor;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            rend.material.color=normalColor;
        }
    }

}
