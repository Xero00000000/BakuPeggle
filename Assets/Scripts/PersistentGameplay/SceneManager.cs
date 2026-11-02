using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{

    private GameObject _player;
    private bool busy;

    private void Awake()
    {
        //_player = GameObject.FindGameObjectWithTag("Player");
    }

    public void SceneTransition(Component sender, SceneField[] scenesToLoad, SceneField[] scenesToUnload, object[] transitionEffects)
    {
        if (busy) return;   // ignore double clicks
        StartCoroutine(TransitionRoutine(scenesToLoad, scenesToUnload));
    }

    private IEnumerator TransitionRoutine(SceneField[] toLoad, SceneField[] toUnload)
    {
        busy = true;
        Time.timeScale = 1f;

        // 1. Load the new scenes first
        var loadOps = new List<AsyncOperation>();
        string firstNew = null;

        if (toLoad != null)
        {
            foreach (var s in toLoad)
            {
                if (s == null || IsLoaded(s.SceneName)) continue;
                var op = SceneManager.LoadSceneAsync(s.SceneName, LoadSceneMode.Additive);
                if (op == null) continue;
                loadOps.Add(op);
                if (firstNew == null) firstNew = s.SceneName;
            }
        }

        while (loadOps.Exists(o => !o.isDone)) yield return null;

        // 2. Make the first new scene active so Peggle is no longer the active/last scene
        if (firstNew != null)
        {
            Scene sc = SceneManager.GetSceneByName(firstNew);
            if (sc.isLoaded) SceneManager.SetActiveScene(sc);
        }

        // 3. Now it's safe to unload
        if (toUnload != null)
        {
            foreach (var s in toUnload)
            {
                if (s == null) continue;
                Scene sc = SceneManager.GetSceneByName(s.SceneName);

                if (!sc.isLoaded)
                {
                    Debug.LogWarning($"Unload requested for '{s.SceneName}' but it isn't loaded. Check the SceneField.", this);
                    continue;
                }
                if (SceneManager.UnloadSceneAsync(sc) == null)
                    Debug.LogWarning($"Unity refused to unload '{s.SceneName}'.", this);
            }
        }

        busy = false;
    }

    private static bool IsLoaded(string name)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).name == name) return true;
        return false;
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
