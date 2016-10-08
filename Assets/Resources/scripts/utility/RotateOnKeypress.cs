using UnityEngine;
using System.Collections;

public class RotateOnKeypress : MonoBehaviour {
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
    {
        float rotateX = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, rotateX);

    }
}
