using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

interface IPausable : IEventSystemHandler
{
    void Pause();
    void Unpause();
}

public class Pausable : MonoBehaviour, IPausable {
    
    private Rigidbody rb;

    public void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }
    
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

    public void Pause()
    {
        rb.Sleep();
    }

    public void Unpause()
    {
        rb.WakeUp();
    }

    public GameObject GameObject()
    {
        return this.gameObject;
    }
}
