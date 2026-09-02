using System.Collections.Generic;
using UnityEngine;


public abstract class EventChannel : ScriptableObject
{
    public List<EventListener> listeners = new();

    //transmitir el evento, los listeners lo van a recivir como una señal de radio; implementar en los hijos
    //public void Raise(Component sender); //se le suele decir raise, pero siento que "broadcast" seria mas correcto lol

    //cosas para gestionar los listeners
    public void RegisterListener(EventListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }
    public void UnregisterListener(EventListener listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }
}

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


