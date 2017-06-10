﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UserInterface
{
    public class UIBase : MonoBehaviour
    {

        private Vector3 pointContext;
        private ActorSystem.Actor actorContext;

        public Camera mycamera;
        public RectTransform uiPanel;

        public RectTransform moveButton;
        public RectTransform attackButton;
        public RectTransform swapButton;
        public RectTransform abilityButton;

        private ActorSystem.LocatableEmptyActionPrototype movePrototype;
        private ActorSystem.SingleTargetDamageActionPrototype attackPrototype;

        // Use this for initialization
        void Start()
        {

            // Create action prototypes --- these will eventually be generated by the Actor

            // attack prototype
            attackPrototype = new ActorSystem.SingleTargetDamageActionPrototype();
            attackPrototype.Cooldown = new Expression("0");
            attackPrototype.Cost = new Dictionary<string, Expression>();
            attackPrototype.Damage = new Dictionary<string, Expression>(); //  { { "health", new Expression("5") } };
            attackPrototype.Range = new Expression("1");
            attackPrototype.Animation = ActorSystem.ActionAnimation.BasicAttack;

            // move prototype
            movePrototype = new ActorSystem.LocatableEmptyActionPrototype();


            uiPanel.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Relocate(Vector3 dest, Vector3 offset)
        {
            uiPanel.anchoredPosition = mycamera.WorldToScreenPoint(dest + offset);
        }

        void ActorSelected(ActorSystem.Actor actor)
        {
            actorContext = actor;
            Vector3 offset = new Vector3(0f, 2f, 0f);
            uiPanel.gameObject.SetActive(true);
            Relocate(actor.Position, offset);

            moveButton.gameObject.SetActive(false);
            abilityButton.gameObject.SetActive(true);
            attackButton.gameObject.SetActive(true);
            swapButton.gameObject.SetActive(true);

            List<RectTransform> buttons = new List<RectTransform>
            {
                attackButton,
                swapButton,
                abilityButton
            };

            ButtonPositioner.layout(buttons);
        }

        void PointSelected(Vector3 point)
        {
            pointContext = point;
            actorContext = null;
            Vector3 offset = Vector3.zero;
            uiPanel.gameObject.SetActive(true);
            Relocate(point, offset);

            moveButton.gameObject.SetActive(true);
            abilityButton.gameObject.SetActive(true);
            attackButton.gameObject.SetActive(false);
            swapButton.gameObject.SetActive(false);

            List<RectTransform> buttons = new List<RectTransform>
            {
                moveButton,
                abilityButton
            };
            ButtonPositioner.layout(buttons);
        }

        // Subscribe/Unsubscribe to our events
        public void OnEnable()
        {
            EventManager.StartListening<ActorSystem.Actor>("ContextClick", ActorSelected);
            EventManager.StartListening<Vector3>("ContextClick", PointSelected);
        }
        public void OnDisable()
        {
            EventManager.StopListening<ActorSystem.Actor>("ContextClick", ActorSelected);
            EventManager.StopListening<Vector3>("ContextClick", PointSelected);
        }

        // buttons
        public void Swap()
        {
            if (actorContext != null)
            {
                EventManager.TriggerEvent("ActorClicked", actorContext);
                actorContext = null;
                uiPanel.gameObject.SetActive(false);
            }
        }

        public void Move()
        {
            Geometry.Locatable target = new Geometry.Locatable();
            if (actorContext == null)
            {
                target.Position = pointContext;
            }
            else
            {
                target.Position = actorContext.Position;
            }
            target.Direction = Vector3.zero;
            ActorSystem.IAction action = movePrototype.Instantiate(GameController.PlayerActor, target);
            EventManager.TriggerEvent("DoAction", GameController.PlayerActor, action);
            uiPanel.gameObject.SetActive(false);
        }

        public void Attack()
        {
            if (actorContext != null)
            {
                ActorSystem.IAction action = attackPrototype.Instantiate(GameController.PlayerActor, actorContext);
                EventManager.TriggerEvent("DoAction", GameController.PlayerActor, action);
                uiPanel.gameObject.SetActive(false);
            }
        }
    }
}
