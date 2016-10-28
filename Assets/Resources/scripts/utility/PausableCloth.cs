using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Cloth))]
public class PausableCloth : MonoBehaviour {

    Cloth cloth;

    void Awake()
    {
        cloth = gameObject.GetComponent<Cloth>();
    }

    // --- EVENT HANDLING --- // 
    // Subscribe/Unsubscribe to our events
    public void OnEnable()
    {
        EventManager.StartListening("Pause", Pause);
        EventManager.StartListening("Unpause", Unpause);
    }
    public void OnDisable()
    {
        EventManager.StopListening("Pause", Pause);
        EventManager.StopListening("Unpause", Unpause);
    }

    float damping;
    bool useGravity;
    float worldAccelerationScale;
    float worldVelocityScale;
    public void Pause()
    {
        damping = cloth.damping;
        useGravity = cloth.useGravity;
        worldAccelerationScale = cloth.worldAccelerationScale;
        worldVelocityScale = cloth.worldVelocityScale;
        cloth.ClearTransformMotion();

        cloth.damping = 1f;
        cloth.useGravity = false;
        cloth.worldVelocityScale = 0f;
        cloth.worldAccelerationScale = 0f;
    }

    public void Unpause()
    {
        cloth.damping = damping;
        cloth.useGravity = useGravity;
        cloth.worldVelocityScale = worldVelocityScale;
        cloth.worldAccelerationScale = worldAccelerationScale;
    }

}
