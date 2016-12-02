using UnityEngine;
using System.Collections;

namespace ActorSystem
{
    public class ActionHandler
    {
        private Actor parent;

        public ActionHandler(Actor parent)
        {
            this.parent = parent;
        }

        public void DoAction(ActionData actionData)
        {
            Debug.Log(parent.gameObject.name + " says OW!");
        }
        
    }
}
