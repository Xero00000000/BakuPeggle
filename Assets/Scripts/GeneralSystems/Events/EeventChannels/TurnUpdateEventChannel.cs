using UnityEngine;

[CreateAssetMenu(fileName = "TurnUpdateEventChannel", menuName = "ScriptableObjects/EventsChannels/TurnUpdateEventChannel")]
public class TurnUpdateEventChannel : EventChannel
{
    public void Raise(Component sender, int[] variables) //se le suele decir raise, pero siento que "broadcast" seria mas correcto lol
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] is TurnUpdateEventListener listener)
            {
                listener.OnEventRaised(sender, variables);
            }
        }
    }
}
