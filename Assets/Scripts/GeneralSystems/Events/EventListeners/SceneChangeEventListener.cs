using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SceneChangeEventListener : EventListener
{
    [SerializeField] private CustomSceneChangeEvent _response;

    public void OnEventRaised(Component sender, SceneField[] scenesToLoad, SceneField[] scenesToUnload, object[] transitionEffects)
    {
        _response.Invoke(sender, scenesToLoad, scenesToUnload, transitionEffects);
    }
}
