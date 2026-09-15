using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{

    private GameObject _player;

    private void Awake()
    {
        //_player = GameObject.FindGameObjectWithTag("Player");
    }

    public void SceneTransition(Component sender, SceneField[] scenesToLoad, SceneField[] scenesToUnload, object[] transitionEffects)
    {
        LoadScenes(scenesToLoad);
        UnLoadScenes(scenesToLoad);
    }

    private void LoadScenes(SceneField[] _scenesToLoad)
    {
        for (int i = 0; i < _scenesToLoad.Length; i++)
        {
            bool isSceneLoaded = false;
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == _scenesToLoad[i].SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }
            }

            if (!isSceneLoaded)
            {
                SceneManager.LoadSceneAsync(_scenesToLoad[i], LoadSceneMode.Additive);
            }
        }
    }

    private void UnLoadScenes(SceneField[] _scenesToUnload)
    {
        for (int i = 0; i < _scenesToUnload.Length; i++)
        {
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == _scenesToUnload[i].SceneName)
                {
                    SceneManager.UnloadSceneAsync(_scenesToUnload[i]);
                }
            }
        }
    }

    //aca cosas probando para ver que me sale
    [SerializeField] private SceneField _Test1;
    [SerializeField] private SceneField _Test2;
    [SerializeField] private SceneField _Test3;
    [SerializeField] private SceneField[] _scenesToLoadTest;
    [SerializeField] private SceneField[] _scenesToUnloadTest;

    public void TestLoadScenes()
    {
        
        SceneManager.LoadSceneAsync(_Test1, LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync(_Test2, LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync(_Test3, LoadSceneMode.Additive);

        for (int i = 0; i < _scenesToLoadTest.Length; i++)
        {
            bool isSceneLoaded = false;
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == _scenesToLoadTest[i].SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }
            }

            if (!isSceneLoaded)
            {
                SceneManager.LoadSceneAsync(_scenesToLoadTest[i], LoadSceneMode.Additive);
            }
        }
    }
}
