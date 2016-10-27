using UnityEngine;
using System.Collections;

public class Zoomable : MonoBehaviour {

    public Transform zoomIn;
    public Transform zoomOut;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        UpdateZoom();
        transform.position = Vector3.Lerp(zoomOut.position, zoomIn.position, cameraDistance);
        transform.rotation = Quaternion.Lerp(zoomOut.rotation, zoomIn.rotation, cameraDistance);
	}

    float scrollVelocity = 0f;
    float lastScroll = 0f;
    float cameraDistance = 0f;
    float cameraSpeed = 30f;
    void UpdateZoom()
    {
        var scroll = Input.GetAxis("LookZoom") * cameraSpeed * Time.deltaTime;
        scroll = Mathf.SmoothDamp(lastScroll, scroll, ref scrollVelocity, 0.1f);

        if (scroll > 0 && cameraDistance <= 1f )
        {
            cameraDistance += scroll;
        }
        if (scroll < 0 && cameraDistance >= 0f )
        {
            cameraDistance += scroll;
        }

        lastScroll = scroll;
    }
}
