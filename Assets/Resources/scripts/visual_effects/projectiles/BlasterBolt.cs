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
        var e = Instantiate<Transform>(explosion, this.transform.position, this.transform.rotation);

        Destroy(this.gameObject);
    }
}
