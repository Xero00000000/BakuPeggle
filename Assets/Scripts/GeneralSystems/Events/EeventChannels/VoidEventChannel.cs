using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "ScriptableObjects/EventsChannels/VoidEventChannel")]
public class VoidEventChannel : EventChannel
{
    public void Raise(Component sender) //se le suele decir raise, pero siento que "broadcast" seria mas correcto lol
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] is VoidEventListener listener)
            {
                listener.OnEventRaised(sender);
            }
        }
    }
}
