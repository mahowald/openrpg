using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Actor
{
    // Who is controlling this character? (The computer, or the player)
    public enum MovementController { Computer, Player};

    // Collect the attributes of the Actor
    // e.g. STR, DEX, MAG, WIL, CUN, CON -- basic attributes
    // as well as combat attributes such as hit points, armor, attack, defense, etc.
    public struct Attributes
    {
        public Dictionary<string, float> attributes; // the actual attributes
        // public Dictionary<string, float> modifiers; // modifiers on the attributes, e.g. +10 def. Removed: Modifiers will be kept track of by StatusEffects. 
        public Attributes(Dictionary<string, float> attributes) // Dictionary<string, float> modifiers)
        {
            this.attributes = attributes;
            // this.modifiers = modifiers;
        }
        public float this[string key]
        {
            get { return attributes[key]; }
            set { attributes[key] = value; }
        }
    }

    [RequireComponent (typeof (NavMeshAgent))]
    // [RequireComponent (typeof (Animator))]
    public class Actor : MonoBehaviour, IPausable {
        
        // The numerical attributes of the actor
        public Attributes attributes; // This should be set on initialization

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

        // Navigation and movement

        public MovementController movementController = MovementController.Computer;

        private NavMeshAgent navAgent;
        private Vector3 destination;

        private Vector3 movingVelocity = Vector3.zero;
        public void Pause()
        {
            navAgent.Stop();
            movingVelocity = navAgent.velocity;
            navAgent.velocity = Vector3.zero;
        }

        public void Unpause()
        {
            navAgent.Resume();
            navAgent.velocity = movingVelocity;
        }

        // Subscribe/Unsubscribe to our events
        public void OnEnable()
        {
            EventManager.StartListening("Pause", Pause);
            EventManager.StartListening("Unpause", Unpause);
            EventManager.StartListening("MouseClickLocation3D", SetClickDestination);
        }
        public void OnDisable()
        {
            EventManager.StopListening("Pause", Pause);
            EventManager.StopListening("Unpause", Unpause);
            EventManager.StopListening("MouseClickLocation3D", SetClickDestination);
        }

        // Where to go when we're in click-to-move
        private void SetClickDestination(Vector3 targetDestination)
        {
            if(movementController == MovementController.Player)
                Destination = targetDestination;
        }
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
            SetKeyboardMovement();
	    }

        private void SetKeyboardMovement()
        {
            if (movementController == MovementController.Player)
            {
                Vector3 moveDir = GameController.Get3DMovementDir();
                if(Vector3.Magnitude(moveDir) > 0.2f)
                {
                    Destination = GameController.Get3DMovementDir() + Position;
                }
            }
        }
    }


}
