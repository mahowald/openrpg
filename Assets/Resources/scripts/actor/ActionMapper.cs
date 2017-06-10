using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace ActorSystem
{

    public enum ActionContext
    {
        None, // no context
        Location, // click on a location
        Item, // click on an item (loot, door, etc)
        Cover, // click on cover
        Actor // click on another actor
    }

    /// <summary>
    /// A class to maintain a mapping of contexts to actions.
    /// </summary>
    public class ActionMapper : Utility.SerializableElement
    {

        // Given a target context, this class should retrieve all of the
        // available actions for that context. 


        // We need to generate an action for a given context
        // so probably this should be IActionPrototype
        [YamlMember(Alias = "actionBag")]
        public Dictionary<ActionContext, List<IActionPrototype>> actionBag { get; set; }

        public ActionMapper()
        {
            actionBag = new Dictionary<ActionContext, List<IActionPrototype>>();
        }

        
        public List<IActionPrototype> this[ActionContext context]
        {
            get
            {
                return actionBag[context];
            }
            set
            {
                actionBag[context] = value;
            }
        }
    
    }

}
