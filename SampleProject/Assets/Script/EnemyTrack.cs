using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTrack : MonoBehaviour
{

    NavMeshAgent navigate;
    GameObject target;

    void Start()
    {
        target = GameObject.Find("PlayerCapsule");
        navigate = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if(navigate.destination != target.transform.position)
        {
            navigate.SetDestination(target.transform.position);
        }
        else
        {
            navigate.SetDestination(transform.position);
        }
    }
}
