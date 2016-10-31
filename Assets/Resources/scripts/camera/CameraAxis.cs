using UnityEngine;
using System;
using System.Collections;

public class CameraAxis : MonoBehaviour {
    
    public float mouseSpeed = 150;
    GameController.ViewMode viewMode = GameController.ViewMode.Standard;
    private Quaternion startRotation;
    private Quaternion alteredRotation;
    private float duration = 0.5f;
    private float elapsed = 0f;

	// Use this for initialization
	void Start () {
        startRotation = transform.localRotation;
        alteredRotation = startRotation;
	}
	
    // Update is called once per frame
    void Update()
    {
        if (viewMode == GameController.ViewMode.Standard)
        {
            if(elapsed < duration)
            {
                transform.localRotation = Quaternion.Slerp(alteredRotation, startRotation, elapsed / duration);
                elapsed += Time.deltaTime;
            }
        }
        else if (viewMode == GameController.ViewMode.FreeLook)
        {
            float rotateY = -1f*Input.GetAxis("Mouse Y");
            float angle = transform.localEulerAngles.x;
            // want this to be between -40 and 40 degrees, resp.
            // BUT: angle is always positive, so [-40, 0] --> [320, 360]
            if (angle > 180)
                angle = angle - 360f;
            if (angle > 40 && rotateY > 0)
                rotateY = 0f;
            if (angle < -40 && rotateY < 0)
                rotateY = 0f;
            transform.RotateAround(transform.position, this.transform.right, mouseSpeed * rotateY * Time.deltaTime);
        }

    }

    void SetFreeLook()
    {
        viewMode = GameController.ViewMode.FreeLook;
    }
    
    void SetStandardLook()
    {
        elapsed = 0f;
        alteredRotation = transform.localRotation;
        viewMode = GameController.ViewMode.Standard;
    }

    public void OnEnable()
    {
        EventManager.StartListening("ViewMode: FreeLook", SetFreeLook);
        EventManager.StartListening("ViewMode: Standard", SetStandardLook);
    }
    public void OnDisable()
    {
        EventManager.StopListening("ViewMode: FreeLook", SetFreeLook);
        EventManager.StopListening("ViewMode: Standard", SetStandardLook);
    }
}
