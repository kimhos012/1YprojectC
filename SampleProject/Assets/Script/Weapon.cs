using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    // Weapon Specification
    public string weaponName;
    public int bulletsPerMag;
    public int bulletsTotal;
    public int currentBullets;
    public float range;
    public float fireRate;

    // Parameters
    private float fireTimer;

    // References
    public Transform shootPoint;

    // Use this for initialization
    private void Start()
    {
        currentBullets = bulletsPerMag;
    }

    // Update is called once per frame
    private void Update()
    {
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }

    public void Fire()
    {
        if (fireTimer < fireRate && currentBullets > 0)
        {
            return;
        }
        Debug.Log("Shot Fired!");
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out hit, range))
            Debug.Log("Hit!");
        currentBullets--;
        fireTimer = 0.0f;
    }
}