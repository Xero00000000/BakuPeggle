using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    //[SerializeField] private SceneField[] _scenesToLoad;
    //[SerializeField] private SceneField[] _scenesToUnload;

    private GameObject _player;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    public void SceneTransition(object sender, params object[] data)
    {
        object[] _sceneArrays = (object[])data;

        SceneField[] _scenesToLoad = (SceneField[])_sceneArrays[0];
        SceneField[] _scenesToUnload = (SceneField[])_sceneArrays[1];

        LoadScenes(_scenesToLoad);
        UnLoadScenes(_scenesToLoad);
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
}

//voy a modificar este codigo para nuestro juego; ademas tengo que usar foreach en ves de for creo, y veo si lo puedo hacer con eventos
