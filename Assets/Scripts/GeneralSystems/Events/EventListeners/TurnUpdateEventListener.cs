using UnityEngine;

public class TurnUpdateEventListener : EventListener
{
    [SerializeField] private TurnUpdateEvent _response;

    public void OnEventRaised(Component sender, int[] variables)
    {
        _response.Invoke(sender, variables);
    }
}
