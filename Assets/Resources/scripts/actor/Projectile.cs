using UnityEngine;
using System.Collections;

namespace ActorSystem
{
    public struct ProjectileData
    {
        public SingleTargetDamageActionPrototype actionPrototype;
        public bool affectedByGravity;
        public float speed;
        public bool ignoreCollisions;
    }

    public class Projectile : MonoBehaviour, IPausable
    {
        public static void Create(ProjectileData projectileData, 
            Actor source, 
            ILocatable target)
        {
            Projectile prefab = Resources.Load<Projectile>("prefabs/ProjectilePrefab"); //TODO: this should be handled somewhere else..
            Projectile p = GameObject.Instantiate<Projectile>(prefab);
            p.source = source;
            p.target = target;
            p.projectileData = projectileData;
            p.transform.position = p.source.Position + new Vector3(0f, 1f, 0f); // TODO: this should be set by the weapon hardpoint
            p.transform.LookAt(p.target.Position + new Vector3(0f, 1f, 0f), Vector3.up);
        }

        Actor source;
        ILocatable target;
        ProjectileData projectileData;

        Vector3 targetPosition;
        Vector3 startPosition;

        float t = 0f; // initial time
        float v0; // vertical velocity
        float g = 3f; // gravity

        // Use this for initialization
        void Start()
        {
            startPosition = this.transform.position;
            targetPosition = target.Position;
            float d = Vector3.Distance(startPosition, targetPosition);
            v0 = 0.5f * g * d / projectileData.speed - (startPosition.y - targetPosition.y) * projectileData.speed/d;
        }

        // Update is called once per frame
        void Update()
        {
            if (paused)
            {
                return;
            }

            t += Time.deltaTime;
            this.transform.position += projectileData.speed*this.transform.forward*Time.deltaTime + (v0 - g*t)*this.transform.up*Time.deltaTime;
        }

        // Called upon collision
        void OnTriggerEnter(Collider other)
        {
            Actor actor = other.GetComponent<Actor>();
            if(actor != null && actor != source)
            {
                SingleTargetDamageAction stda = (SingleTargetDamageAction)projectileData.actionPrototype.Instantiate(source, actor);
                var targetData = stda.GenerateTargetActionData();
                actor.HandleAction(targetData);
            }
        }

        private bool paused = false;
        public void Pause()
        {
            paused = true;
        }

        public void Unpause()
        {
            paused = false;
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
    }

}
