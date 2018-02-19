using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEffectTester : MonoBehaviour {

    public Transform projectile;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
            SpawnProjectile();
	}

    void SpawnProjectile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {

            Instantiate<Transform>(projectile, transform.position, Quaternion.LookRotation(hit.point - transform.position, Vector3.up));

        }
    }
}
