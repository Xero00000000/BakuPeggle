using UnityEngine;

public class ShotEventListener : EventListener
{
    [SerializeField] private CustomShotEvent _response;

    public void OnEventRaised(Component sender, int shotFired)
    {
        _response.Invoke(sender, shotFired);
    }
}
