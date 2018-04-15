using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlasterBolt : MonoBehaviour {

    Transform t;
    float velocity = 15f;
    public Transform explosion;

	// Use this for initialization
	void Start () {
        t = this.gameObject.GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position += velocity*Time.deltaTime*this.transform.forward;
	}

    private void OnTriggerEnter(Collider other)
    {
        RaycastHit hit;
        Ray ray = new Ray(this.transform.position, this.transform.forward);

        Vector3 normal = this.transform.forward;

        if (Physics.Raycast(ray, out hit))
        {
            normal = hit.normal;
        }

        var e = Instantiate<Transform>(explosion, this.transform.position, Quaternion.LookRotation(-1f*normal, Vector3.up));

        Destroy(this.gameObject);
    }
}
