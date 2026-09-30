using UnityEngine;

public class LoadScenes : MonoBehaviour
{
    [SerializeField] private SceneChangeEventChannel eventToBroadcast;
    [SerializeField] private SceneField[] scenesToLoad;
    [SerializeField] private SceneField[] scenesToUnload;
    [SerializeField] private object[] talVesCambieEsto;

    public void RaiseEvent()
    {
        eventToBroadcast.Raise(this, scenesToLoad, scenesToUnload, talVesCambieEsto);
    }
}
