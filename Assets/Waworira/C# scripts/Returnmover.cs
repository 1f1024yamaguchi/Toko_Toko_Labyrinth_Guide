

using UnityEngine;

public class Returnmover : MonoBehaviour
{
    Vector3 startposi;
    public float maxdistance;
    public float upspeed;
    bool goingup=true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startposi=transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //float distance=Mathf.Abs(startposi.x-transform.position.x);
        if (goingup)
        {
            transform.Translate(Vector3.forward*upspeed);
            if (transform.position.z >= maxdistance+startposi.z)
            {
                goingup=false;
            }
        }
        else
        {
            transform.Translate(Vector3.back*upspeed);
            if (transform.position.z <= startposi.z-maxdistance)
            {
                goingup=true;
            }
        }
    }
}
