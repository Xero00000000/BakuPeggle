using UnityEngine;

[CreateAssetMenu(fileName = "ShotEventChannel", menuName = "ScriptableObjects/EventsChannels/ShotEventChannel")]
public class ShotEventChannel : EventChannel
{
    public void Raise(Component sender, int shotFired) //se le suele decir raise, pero siento que "broadcast" seria mas correcto lol
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] is ShotEventListener listener)
            {
                listener.OnEventRaised(sender, shotFired);
            }
        }
    }
}
