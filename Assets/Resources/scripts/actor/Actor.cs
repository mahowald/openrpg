using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour {

    // The current position of the actor
    public Vector3 Position
    {
        get { return this.transform.position; }
    }

    // The current destination of the actor
    // Setting updates the nav agent as well. 
    public Vector3 Destination
    {
        get { return this.destination; }
        set
        {
            this.destination = value;
            navAgent.destination = this.destination;
        }
    }

    // The direction of the actor
    public Vector3 Direction
    {
        get { return this.transform.forward; }
        set
        {
            this.transform.forward = value;
        }
    }

    private NavMeshAgent navAgent;
    private Vector3 destination;

    // Called after instantiation, before Start 
    void Awake()
    {
        navAgent = this.gameObject.GetComponent<NavMeshAgent>();
    }
    
    // Called before the first update
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}
}
