using UnityEngine;
using System.Collections;

namespace ActorSystem
{
    public class ActionHandler
    {
        private Actor parent;
        public IAction<ILocatable> queuedAction = null;

        public ActionHandler(Actor parent)
        {
            this.parent = parent;
        }

        public void HandleAction(ActionData actionData)
        {
            // Handle attributes first
            if(actionData.attributeModifier != null)
            {
                foreach(string attribute in actionData.attributeModifier.Keys)
                {
                    parent.attributes[attribute] += actionData.attributeModifier[attribute];
                }
            }

            // Next we do effects
            if(actionData.effects != null)
            {
                foreach(Effect e in actionData.effects.Keys)
                {
                    // TODO: add the effect.
                    // Dictionary is Effect:Duration (float)
                }
            }

            Debug.Log(parent.gameObject.name + " says OW!");
        }
    }
    
}
