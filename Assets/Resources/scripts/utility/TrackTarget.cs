using UnityEngine;
using System.Collections;

public class TrackTarget : MonoBehaviour {

    public Transform follow;

    private float smoothTime = 0.05f;

	// Use this for initialization
	void Start () {
	
	}

    private Vector3 velocity;

	// Update is called once per frame
	void Update () {
        this.transform.position = Vector3.SmoothDamp(transform.position, follow.position, ref velocity, smoothTime);
    }
}
