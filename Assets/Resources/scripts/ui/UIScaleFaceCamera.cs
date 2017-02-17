using UnityEngine;
using System.Collections;

public class UIScaleFaceCamera : MonoBehaviour {

    public Camera activeCamera;

    private Vector3 initialScale;
    private float initialDist;

	// Use this for initialization
	void Start () {
        initialScale = this.transform.localScale;
        initialDist = Vector3.Magnitude(this.transform.position - activeCamera.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
        // this.transform.forward = this.transform.position - activeCamera.transform.position;
        this.transform.LookAt(activeCamera.transform.position, Vector3.up);
        this.transform.forward = -1f * this.transform.forward;

        // Uncomment to scale UI
        // float dist = Vector3.Magnitude(this.transform.position - activeCamera.transform.position);
        // this.transform.localScale = dist / initialDist * initialScale;
	}
}
