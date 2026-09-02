using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

//creo nuestro propio tipo de game event para poder mandar mas parametros
[System.Serializable]
public class CustomVoidEvent : UnityEvent <Component> { }
[System.Serializable]
public class CustomSceneChangeEvent : UnityEvent<Component, SceneField[], SceneField[], object[]> { } //agregar algo para efectos de cambio de escena
[System.Serializable]
public class CustomAAAEvent : UnityEvent<Component, object[]> { }

public abstract class EventListener : MonoBehaviour
{
    [SerializeField] private EventChannel _eventChannel;
    //[SerializeField] private CustomVoidEvent _response; //esto hay que implementar en todos los hijos

    private void OnEnable()
    {
        _eventChannel.RegisterListener(this);
    }
    private void OnDisable()
    {
        _eventChannel.UnregisterListener(this);
    }

    //public void OnEventRaised(Component sender); //hay que implementar esto en todos los hijos tambien
}
