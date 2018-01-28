# A listing of all events handled by the Event Manager

## Generic Events

- "Pause": pauses any listening objects.
- "Unpause": unpauses any listening objects.
- "ViewMode: FreeLook": Free look view mode
- "ViewMode: Standard": Standard view mode

## Bool events
- "Orthographic Mode": True = orthographic mode, False = exiting orthographic mode

## Actor Events

- "ActorClicked": Function with arguments <Actor>. Triggered on Actor click.
- "DoAction": Function with signature <Actor, IAction>. Actors listen to this function to determine what action to perform.
