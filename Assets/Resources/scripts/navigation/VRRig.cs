using UnityEngine;
using System.Collections;
using Valve.VR;

// Handle all VR-specific inputs
public class VRRig : MonoBehaviour {

    private float scale0 = 1.0f;
    private float scale1 = 5.0f;
    private float currentScale = 1.0f;

    public Transform follow;
    private float smoothTime = 0.05f;

    public SteamVR_TrackedObject mainHand;
    public SteamVR_TrackedObject offHand;

    private SteamVR_Controller.Device mainController {
        get
        {
            try
            {
                return SteamVR_Controller.Input((int)mainHand.index);
            }
            catch (System.IndexOutOfRangeException)
            {
                return null;
            }
        }
    }
    private SteamVR_Controller.Device offController {
        get
        {
            try
            {
                return SteamVR_Controller.Input((int)offHand.index);
            }
            catch (System.IndexOutOfRangeException)
            {
                return null;
            }
        }
    }

    public static Vector3 mousePosition = Vector3.zero;

    public static VRRig instance = null;

    // Use this for initialization
    void Start () {
        if (instance != null)
            Debug.LogError("There can only be one VRRig instance!");
        else
            VRRig.instance = this;
	}

    // private Vector3 velocity;

    // Update is called once per frame
    void Update ()
    {
        // THIS is nauseating!
        // this.transform.position = Vector3.SmoothDamp(transform.position, follow.position, ref velocity, smoothTime);

        if (OpenVR.System == null)
            return;

        DoMainHand();
        DoOffHand();

    }

    void DoOffHand()
    {
        if(offController == null)
        {
            Debug.Log("Controller not initialized");
            return;
        }
        if (offController.GetPress(SteamVR_Controller.ButtonMask.Trigger) && offController.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            float input = offController.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;
            currentScale = Mathf.Clamp(currentScale - Mathf.Sign(input)*1f, scale0, scale1); // we move in discrete steps to avoid nausea
            transform.localScale = currentScale * Vector3.one;
        }
        else if (offController.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            this.transform.position = follow.position;
        }
    }

    public static Vector2 GetOffHandMoveVector()
    {
        if (instance.offController == null)
        {
            Debug.Log("Controller not initialized");
            return Vector2.zero;
        }

        if (!instance.offController.GetPress(SteamVR_Controller.ButtonMask.Touchpad) || instance.offController.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            return Vector2.zero;

        float vertical = instance.offController.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;
        float horizontal = instance.offController.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x;

        return new Vector2(horizontal, vertical);
    }

    ActorSystem.Actor lastActor = null;
    void DoMainHand()
    {
        if(mainController == null)
        {
            Debug.Log("Controller not initialized");
            return;
        }

        Ray ray = new Ray(mainHand.transform.position, mainHand.transform.forward);
        RaycastHit hit;
        bool raycast = Physics.Raycast(ray, out hit);
        ActorSystem.Actor actor = null;
        if(raycast)
        {
            VRRig.mousePosition = hit.point;

            actor = hit.transform.GetComponent<ActorSystem.Actor>();
            if(actor != null)
            {
                actor.Highlighted = true;
                if (actor != lastActor && lastActor != null)
                    lastActor.Highlighted = false;
                lastActor = actor;
            }
            else
            {
                if(lastActor != null)
                    lastActor.Highlighted = false;
                lastActor = null;
            }
        }
        else
        {
            if (lastActor != null)
                lastActor.Highlighted = false;
            lastActor = null;
        }

        if (mainController.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (raycast)
            {
                if (actor != null)
                {
                    EventManager.TriggerEvent<ActorSystem.Actor>("ActorClicked", actor);
                }
                else // check that we didn't click on an actor
                    EventManager.TriggerEvent<Vector3>("MouseClickLocation3D", hit.point);

            }

        }
    }

    void SetFollowActor(ActorSystem.Actor a)
    {
        follow = a.transform;
    }

    public void OnEnable()
    {
        EventManager.StartListening<ActorSystem.Actor>("ActorClicked", SetFollowActor);
    }
    public void OnDisable()
    {
        EventManager.StopListening<ActorSystem.Actor>("ActorClicked", SetFollowActor);
    }
}
