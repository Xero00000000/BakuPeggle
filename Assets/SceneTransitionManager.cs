using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Material transitionMaterial;

    [Header("Transición")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    private static readonly int ProgressProperty = Shader.PropertyToID("_Progress");

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        transitionMaterial.SetFloat(ProgressProperty, 1f);
    }

    public void LoadNewScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        transitionMaterial.SetFloat(ProgressProperty, 0f);

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);

            transitionMaterial.SetFloat(ProgressProperty, t * 0.5f);
            yield return null;
        }

        transitionMaterial.SetFloat(ProgressProperty, 0.5f);
        yield return new WaitForSeconds(0.1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);

            transitionMaterial.SetFloat(ProgressProperty, 0.5f + (t * 0.5f));
            yield return null;
        }

        transitionMaterial.SetFloat(ProgressProperty, 1.0f);
    }
}