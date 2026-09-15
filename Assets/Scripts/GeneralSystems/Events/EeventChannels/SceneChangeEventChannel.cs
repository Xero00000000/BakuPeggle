using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneChangeEventChannel", menuName = "ScriptableObjects/EventsChannels/SceneChangeEventChannel")]
public class SceneChangeEventChannel : EventChannel
{
    public void Raise(Component sender, SceneField[] scenesToLoad, SceneField[] scenesToUnload, object[] transitionEffects) //se le suele decir raise, pero siento que "broadcast" seria mas correcto lol
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] is SceneChangeEventListener listener)
            {
                listener.OnEventRaised(sender, scenesToLoad, scenesToUnload, transitionEffects);
            }
        }
    }
}
