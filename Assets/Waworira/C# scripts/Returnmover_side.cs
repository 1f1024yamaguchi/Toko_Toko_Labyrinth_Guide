

using UnityEngine;

public class Returnmover_side : MonoBehaviour
{
    Vector3 startposi;
    public float maxdistance;
    public float sidespeed;
    bool goingside=true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startposi=transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //float distance=Mathf.Abs(startposi.x-transform.position.x);
        if (goingside)
        {
            transform.Translate(Vector3.right*sidespeed);
            if (transform.position.x >= maxdistance+startposi.x)
            {
                goingside=false;
            }
        }
        else
        {
            transform.Translate(Vector3.left*sidespeed);
            if (transform.position.x <= startposi.x-maxdistance)
            {
                goingside=true;
            }
        }
    }
}
