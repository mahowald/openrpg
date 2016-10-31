using UnityEngine;
using System.Collections;

public class CameraBase : MonoBehaviour {

    public float speed = 50;
    public float mouseSpeed = 150;
    GameController.ViewMode viewMode = GameController.ViewMode.Standard;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(viewMode == GameController.ViewMode.Standard)
        {
            float rotateX = Input.GetAxis("HorizontalAlt");
            transform.Rotate(Vector3.up, speed * rotateX * Time.deltaTime);
        } else if (viewMode == GameController.ViewMode.FreeLook)
        {
            float rotateX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseSpeed * rotateX * Time.deltaTime);
        }

    }

    void SetFreeLook()
    {
        viewMode = GameController.ViewMode.FreeLook;
    }

    void SetStandardLook()
    {
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
