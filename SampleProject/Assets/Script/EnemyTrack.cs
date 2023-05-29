using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTrack : MonoBehaviour
{

    NavMeshAgent navi;
    GameObject target;

    void Start()
    {
        target = GameObject.Find("PlayerCapsule");
        navi = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if(navi.destination != target.transform.position)
        {
            navi.SetDestination(target.transform.position);
        }
        else
        {
            navi.SetDestination(transform.position);
        }
    }
}
