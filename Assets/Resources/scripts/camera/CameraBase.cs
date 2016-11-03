using UnityEngine;
using ActorSystem;
using System.Collections;

public class CameraBase : MonoBehaviour {

    public float speed = 50;
    public float mouseSpeed = 150;
    GameController.ViewMode viewMode = GameController.ViewMode.Standard;
    public Transform follow;

    private float smoothTime = 0.05f;

    // Use this for initialization
    void Start()
    {
    }

    private Vector3 velocity;
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

        this.transform.position = Vector3.SmoothDamp(transform.position, follow.position, ref velocity, smoothTime);
    }

    void SetFreeLook()
    {
        viewMode = GameController.ViewMode.FreeLook;
    }

    void SetStandardLook()
    {
        viewMode = GameController.ViewMode.Standard;
    }

    void SetFollowActor(Actor a)
    {
        follow = a.transform;
    }

    public void OnEnable()
    {
        EventManager.StartListening("ViewMode: FreeLook", SetFreeLook);
        EventManager.StartListening("ViewMode: Standard", SetStandardLook);
        EventManager.StartListening<Actor>("ActorClicked", SetFollowActor);
    }
    public void OnDisable()
    {
        EventManager.StopListening("ViewMode: FreeLook", SetFreeLook);
        EventManager.StopListening("ViewMode: Standard", SetStandardLook);
        EventManager.StopListening<Actor>("ActorClicked", SetFollowActor);
    }
}
