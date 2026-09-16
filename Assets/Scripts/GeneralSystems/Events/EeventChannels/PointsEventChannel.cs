using UnityEngine;

[CreateAssetMenu(fileName = "PointsEventChannel", menuName = "ScriptableObjects/EventsChannels/PointsEventChannel")]
public class PointsEventChannel : EventChannel
{
    public void Raise(Component sender, bool player, int pointsAdded) //se le suele decir raise, pero siento que "broadcast" seria mas correcto lol
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] is PointsEventListener listener)
            {
                listener.OnEventRaised(sender, player, pointsAdded);
            }
        }
    }
}
