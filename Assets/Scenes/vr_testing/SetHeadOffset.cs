using UnityEngine;
using System.Collections;

public class SetHeadOffset : MonoBehaviour {

    public GameObject head;
    public GameObject vrHead;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
        Vector3 offset = head.transform.position - this.transform.position;
        this.transform.position = vrHead.transform.position - offset;

	}
}
