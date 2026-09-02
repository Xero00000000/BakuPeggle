using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class VoidEventListener : EventListener
{
    [SerializeField] private CustomVoidEvent _response;

    public void OnEventRaised(Component sender)
    {
        _response.Invoke(sender);
    }
}
