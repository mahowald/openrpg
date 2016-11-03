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
    [RequireComponent (typeof (Animator))]
    [RequireComponent (typeof (Rigidbody))]
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
        // --- GAME LOGIC --- //

        public void AddEffect(Effect effect, float duration)
        {
            return;
        }

        // --- NAVIGATION AND MOVEMENT --- //

        public MovementController movementController = MovementController.Computer;

        private NavMeshAgent navAgent;
        private Animator animator;
        private Vector3 destination;
        private Rigidbody rbody;

        // Where to go when we're in click-to-move
        private void SetClickDestination(Vector3 targetDestination)
        {
            if (movementController == MovementController.Player)
                Destination = targetDestination;
        }

        // Set the destination based on keyboard movement
        private bool SetKeyboardMovement()
        {
            if (movementController == MovementController.Player)
            {
                Vector3 moveDir = GameController.Get3DMovementDir();
                NavMeshHit hit;
                bool validPosition = NavMesh.SamplePosition(transform.position + moveDir, out hit, 1f, NavMesh.AllAreas);
                if (!validPosition)
                    moveDir = Vector3.zero;
                else
                    moveDir = hit.position - transform.position;
                if (Vector3.Magnitude(moveDir) > 0.1f)
                {
                    Move(moveDir);
                    Destination = GameController.Get3DMovementDir() + Position;
                    return true;
                }
                return false;
            }
            return false;
        }

        float m_ForwardAmount;
        float m_TurnAmount;
        public void Move(Vector3 move)
        {
            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            move = Vector3.ProjectOnPlane(move, Vector3.up); // TODO: update for ground normal
            m_TurnAmount = Mathf.Atan2(move.x, move.z);
            m_ForwardAmount = move.z;

            ApplyExtraTurnRotation();
            // send input and other state parameters to the animator
            UpdateAnimator(move);
        }

        float m_MovingTurnSpeed = 360;
        float m_StationaryTurnSpeed = 180;
        float m_MoveSpeedMultiplier = 1f;
        float m_AnimSpeedMultiplier = 1f;
        void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
        }
        void UpdateAnimator(Vector3 move)
        {
            if (!paused)
            {
                // update the animator parameters
                animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
                animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            }
        }

        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (Time.deltaTime > 0)
            {
                Vector3 v = (animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                v.y = rbody.velocity.y;
                rbody.velocity = v;

                transform.position = animator.rootPosition;
            }
        }

        // Scale the velocity as we near the target, so that we stop smoothly
        private float slowDist = 2f;
        private float slowSpeed = 0.25f;
        private float ScaledVelocity()
        {
            float scale = 1f;
            float distance = Vector3.Magnitude(transform.position - this.Destination);
            if (distance < slowDist)
            {
                scale = (distance / slowDist)*(1 - slowSpeed) + slowSpeed;
            }
            return scale;
        }

        // --- PAUSING AND UNPAUSING --- //
        private Vector3 movingVelocity = Vector3.zero;
        private float animatorSpeed = 1f;
        bool paused = false;
        public void Pause()
        {
            navAgent.Stop();
            animatorSpeed = animator.speed;
            animator.speed = 0; // pause the animator
            movingVelocity = navAgent.velocity;
            navAgent.velocity = Vector3.zero;
            paused = true;
        }

        public void Unpause()
        {
            navAgent.Resume();
            animator.speed = animatorSpeed; // unpause the animator
            navAgent.velocity = movingVelocity;
            paused = false;
        }

        // --- EVENT HANDLING --- // 
        // Subscribe/Unsubscribe to our events
        public void OnEnable()
        {
            EventManager.StartListening("Pause", Pause);
            EventManager.StartListening("Unpause", Unpause);
            EventManager.StartListening<Vector3>("MouseClickLocation3D", SetClickDestination);
        }
        public void OnDisable()
        {
            EventManager.StopListening("Pause", Pause);
            EventManager.StopListening("Unpause", Unpause);
            EventManager.StopListening<Vector3>("MouseClickLocation3D", SetClickDestination);
        }


        // --- INITIALIZATION --- //
        // Called after instantiation, before Start 
        void Awake()
        {
            navAgent = this.gameObject.GetComponent<NavMeshAgent>();
            animator = this.gameObject.GetComponent<Animator>();
            rbody = this.gameObject.GetComponent<Rigidbody>();
            
        }
    
        // Called before the first update
	    void Start () {
	
	    }
	
        // --- UPDATING --- //
	    // Update is called once per frame
	    void Update () {
            if(!paused)
            {
                bool keyboardMovementSet = SetKeyboardMovement();
                if (!keyboardMovementSet)
                {
                    if (navAgent.remainingDistance > navAgent.stoppingDistance)
                        Move(ScaledVelocity()*navAgent.desiredVelocity/navAgent.speed);
                    else
                        Move(Vector3.zero);
                }

                navAgent.updatePosition = false;
                // navAgent.updateRotation = true;
                navAgent.nextPosition = transform.position;
            }
            
        }

       
    }


}
