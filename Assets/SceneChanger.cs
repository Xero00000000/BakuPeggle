using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneChanger : MonoBehaviour
{
    [Header("Selección de Escena Individual")]
#if UNITY_EDITOR
    public SceneAsset sceneAsset;
#endif
    public string sceneName;

    [Header("Efecto Transición (Shader HLSL)")]
    public Material transitionMaterial;
    public float transitionDuration = 0.8f;

    private RawImage overlayImage;
    private GameObject canvasObj;
    private bool isLoading = false;

    [Header("Múltiples Escenas (Additive)")]
    [SerializeField] private SceneField[] _scenesToLoad;
    [SerializeField] private SceneField[] _scenesToUnload;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null) sceneName = sceneAsset.name;
    }
#endif

    private void Start()
    {
        SetupOverlayCanvas();

        // Al entrar a una nueva escena, inicia opaco (1) y revela el contenido (0)
        if (transitionMaterial != null)
        {
            StartCoroutine(AnimateTransition(1f, 0f, transitionDuration / 2f));
        }
    }

    private void SetupOverlayCanvas()
    {
        if (transitionMaterial == null) return;

        canvasObj = new GameObject("SceneTransitionOverlayCanvas");
        // NOTA: NO usamos DontDestroyOnLoad. Este Canvas morirá con la escena actual.

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
    }

    public void NewLoad()
    {
        if (isLoading) return;
        StartCoroutine(LoadMultipleScenesAsyncCoroutine());
    }

    private IEnumerator LoadMultipleScenesAsyncCoroutine()
    {
        isLoading = true;
        float halfDuration = transitionDuration / 2f;

        // 1. Ocultar la escena actual (Shader 0 a 1)
        yield return StartCoroutine(AnimateTransition(0f, 1f, halfDuration));

        // 2. Preparar lista de descarga
        List<AsyncOperation> unloadOperations = new List<AsyncOperation>();

        foreach (SceneField scene in _scenesToUnload)
        {
            if (scene != null && !string.IsNullOrEmpty(scene.SceneName))
            {
                Scene loadedScene = SceneManager.GetSceneByName(scene.SceneName);
                if (loadedScene.isLoaded)
                {
                    AsyncOperation op = SceneManager.UnloadSceneAsync(loadedScene);
                    if (op != null) unloadOperations.Add(op);
                }
            }
        }

        // 3. Cargar las nuevas escenas
        List<AsyncOperation> loadOperations = new List<AsyncOperation>();
        foreach (SceneField scene in _scenesToLoad)
        {
            if (scene != null && !string.IsNullOrEmpty(scene.SceneName))
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
                if (op != null)
                {
                    op.allowSceneActivation = false;
                    loadOperations.Add(op);
                }
            }
        }

        // Esperar a que carguen al 90%
        bool allLoaded = false;
        while (!allLoaded)
        {
            allLoaded = true;
            foreach (var op in loadOperations)
            {
                if (op.progress < 0.9f) { allLoaded = false; break; }
            }
            yield return null;
        }

        // Activar escenas
        foreach (var op in loadOperations) op.allowSceneActivation = true;

        while (loadOperations.Exists(op => !op.isDone)) yield return null;

        // 4. Finalmente, descargar la escena de origen. 
        // ¡Atención! Esta escena y este script SE DESTRUIRÁN en la siguiente línea.
        // La nueva escena (que tiene su propio SceneChanger) ejecutará su Start() y hará el Fade Out.
        Scene currentScene = gameObject.scene;
        if (currentScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(currentScene);
        }
    }

    private IEnumerator AnimateTransition(float startVal, float endVal, float duration)
    {
        if (transitionMaterial == null || duration <= 0) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Lerp(startVal, endVal, elapsed / duration);
            transitionMaterial.SetFloat("_Progress", progress);
            yield return null;
        }
        transitionMaterial.SetFloat("_Progress", endVal);
    }
}