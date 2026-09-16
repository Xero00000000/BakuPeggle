using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class RToReload : MonoBehaviour
{
    [Header("Escenas a Recargar")]
    [SerializeField] private SceneField peggle;
    [SerializeField] private SceneField launchers;

    private bool isReloading = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            StartCoroutine(ReloadScenesRoutine());
        }
    }

    private IEnumerator ReloadScenesRoutine()
    {
        isReloading = true;

        AsyncOperation unloadPeggle = SceneManager.UnloadSceneAsync(peggle);
        //AsyncOperation unloadLaunchers = SceneManager.UnloadSceneAsync(launchers);

        if (unloadPeggle != null)
            yield return new WaitUntil(() => unloadPeggle.isDone);

        //if (unloadLaunchers != null)
            //yield return new WaitUntil(() => unloadLaunchers.isDone);
        AsyncOperation loadPeggle = SceneManager.LoadSceneAsync(peggle, LoadSceneMode.Additive);
        //AsyncOperation loadLaunchers = SceneManager.LoadSceneAsync(launchers, LoadSceneMode.Additive);

        if (loadPeggle != null)
            yield return new WaitUntil(() => loadPeggle.isDone);

        //if (loadLaunchers != null)
            //yield return new WaitUntil(() => loadLaunchers.isDone);

        isReloading = false;
    }
}