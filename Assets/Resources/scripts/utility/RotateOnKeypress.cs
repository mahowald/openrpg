using UnityEngine;
using System.Collections;

public class RotateOnKeypress : MonoBehaviour {

    public float speed = 50;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
    {
        float rotateX = Input.GetAxis("HorizontalAlt");
        transform.Rotate(Vector3.up, speed*rotateX*Time.deltaTime);

    }
}
