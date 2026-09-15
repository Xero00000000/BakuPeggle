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




