using UnityEngine;

public class PointsEventListener : EventListener
{
    [SerializeField] private CustomPointsEvent _response;

    public void OnEventRaised(Component sender, bool player, int pointsAdded)
    {
        _response.Invoke(sender, player, pointsAdded);
    }
}
