using UnityEngine;

public class TestingEvents : MonoBehaviour
{
    [SerializeField] private SceneChangeEventChannel _sceneTransition;

    [SerializeField] private SceneField[] _scenesToLoad;
    [SerializeField] private SceneField[] _scenesToUnload;
    [SerializeField] private object[] _aaa = new object[4];

    public void SceneTransition()
    {
        _sceneTransition.Raise(this, _scenesToLoad, _scenesToUnload, _aaa);
    }
}
