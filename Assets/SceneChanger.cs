using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneChanger : MonoBehaviour
{
    [Header("Selección de Escena")]
#if UNITY_EDITOR
    [Tooltip("Arrastra aquí el archivo de la escena (solo funciona en el Editor).")]
    public SceneAsset sceneAsset;
#endif
    [Tooltip("Nombre exacto de la escena en Build Settings.")]
    public string sceneName;

    [Header("Efecto Transición (Shader HLSL)")]
    public Material transitionMaterial;
    public float transitionDuration = 0.8f;

    private RawImage overlayImage;
    private bool isLoading = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
    }
#endif

    private void Start()
    {
        SetupOverlayCanvas();
    }

    private void SetupOverlayCanvas()
    {
        if (transitionMaterial == null) return;

        GameObject canvasObj = new GameObject("SceneTransitionOverlayCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasObj.AddComponent<CanvasScaler>();

        GameObject imgObj = new GameObject("TransitionRawImage");
        imgObj.transform.SetParent(canvasObj.transform, false);

        overlayImage = imgObj.AddComponent<RawImage>();
        overlayImage.material = transitionMaterial;

        RectTransform rect = overlayImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        transitionMaterial.SetFloat("_Progress", 0f);
    }
    public void LoadScene()
    {
        LoadSceneByName(sceneName);
    }

    public void LoadSceneByName(string nameToLoad)
    {
        if (isLoading) return;

        if (!string.IsNullOrEmpty(nameToLoad))
        {
            StartCoroutine(LoadSceneAsyncCoroutine(nameToLoad));
        }
        else
        {
            Debug.LogError("[SceneChanger] El nombre de la escena está vacío.", this);
        }
    }

    public void LoadSceneByIndex(int buildIndex)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneAsyncCoroutine(buildIndex));
    }

    private IEnumerator LoadSceneAsyncCoroutine(object sceneIdentifier)
    {
        isLoading = true;

        float elapsed = 0f;
        float halfDuration = transitionDuration / 2f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / halfDuration);
            if (transitionMaterial != null) transitionMaterial.SetFloat("_Progress", progress);
            yield return null;
        }

        AsyncOperation asyncLoad = null;

        if (sceneIdentifier is string name)
        {
            asyncLoad = SceneManager.LoadSceneAsync(name);
        }
        else if (sceneIdentifier is int index)
        {
            asyncLoad = SceneManager.LoadSceneAsync(index);
        }

        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            asyncLoad.allowSceneActivation = true;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(1f - (elapsed / halfDuration));
            if (transitionMaterial != null) transitionMaterial.SetFloat("_Progress", progress);
            yield return null;
        }

        if (transitionMaterial != null) transitionMaterial.SetFloat("_Progress", 0f);
        isLoading = false;
    }
}