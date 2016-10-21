using UnityEngine;
using System.Collections;

public class TrackTarget : MonoBehaviour {

    public Transform follow;

    public float smooth = 5f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = Vector3.Lerp(this.transform.position, follow.position, Time.deltaTime * smooth);
    }
}
