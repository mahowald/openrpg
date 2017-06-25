using UnityEngine;
using System.Collections;

namespace ActorSystem
{
    public struct ProjectileData
    {
        public SingleTargetDamageActionPrototype actionPrototype;
        public bool affectedByGravity;
        public float speed;
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

        // Use this for initialization
        void Start()
        {
            startPosition = this.transform.position;
            targetPosition = target.Position;
        }

        // Update is called once per frame
        void Update()
        {
            if (paused)
            {
                return;
            }


            this.transform.position += projectileData.speed*this.transform.forward*Time.deltaTime;
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
