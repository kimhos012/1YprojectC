using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Playerraycast : MonoBehaviour
{

    RaycastHit hit;
    float rayDistance = 15f;

    // Update is called once per frame
    void Update()
    {



    }

    public void RayShoot()
    {
        Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.blue, 0.3f);


    }
}
