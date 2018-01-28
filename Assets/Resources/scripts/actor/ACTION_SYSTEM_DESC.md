IActionPrototype : an interface that defines a few things:

- Name
- Deserialize (create an action prototype from a YAML string)
- Instatiate<T> (create an instance of this action for a given target)
- Allowed(Actor source) : is the Actor allowed to perform this action
- Valid<T> (T target) : check if target is a valid target for this action


# How the system works:

1. For each Actor, there is a collection of ActionPrototypes. 
2. The player/AI selects an ActionPrototype
3. The player/AI selects a valid target for the ActionPrototype
4. If the current Actor is allowed to perform the Action, and the target is valid, then an Action is Instantiated from
   the ActionPrototype
5. The instantiated Action is added to the Actor's action queue.
6. During the Actor's update phase, the Actor will attempt to move into Range of the Action.
7. Once within Range, the Action will be popped from the Queue, and Action.DoAction() will be called.
8. The DoAction() method will apply effects to both the Source Actor and potentially to the target.
