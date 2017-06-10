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
    public class ActionMapper : object
    {

        // Given a target context, this class should retrieve all of the
        // available actions for that context. 


        // We need to generate an action for a given context
        // so probably this should be IActionPrototype
        private Dictionary<ActionContext, List<IActionPrototype>> actionBag { get; set; }

        public ActionMapper()
        {
            actionBag = new Dictionary<ActionContext, List<IActionPrototype>>();
            PopulateDefaultActions();
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

        private void PopulateDefaultActions()
        {
            var attackPrototype = new SingleTargetDamageActionPrototype();
            attackPrototype.Cooldown = new Expression("0");
            attackPrototype.Cost = new Dictionary<string, Expression>();
            attackPrototype.Damage = new Dictionary<string, Expression>(); //  { { "health", new Expression("5") } };
            attackPrototype.Range = new Expression("1");
            attackPrototype.Animation = ActionAnimation.BasicAttack;

            // move prototype
            var movePrototype = new LocatableEmptyActionPrototype();

            actionBag[ActionContext.Location] = new List<IActionPrototype>() { movePrototype };
            actionBag[ActionContext.Actor] = new List<IActionPrototype>() { attackPrototype };

        }
    
    }

}
