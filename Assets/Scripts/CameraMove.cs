using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

// Necesario para el arrastre de la escena en el Inspector
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraMove : MonoBehaviour
{
    [Header("Camara")]
    public Camera _camera;
    public float _closeDistance = 2f;
    public float moveSpeed = 5f;

    [Header("Offset")]
    public float offsetX = 0f;
    public float offsetY = 0f;
    public float offsetZ = 0f;

    [Header("SCENE")]
#if UNITY_EDITOR
    public SceneAsset toChrage;
#endif
    [Header("Keycode")]
    [SerializeField] KeyCode _keyCode;

    [SerializeField] public string sceneName;

    [Header("Configuracion de Transición")]
    [Tooltip("Asigna el Shader 'UI/DiamondTransition' o déjalo vacío para que lo busque por nombre")]
    [SerializeField] private Shader transitionShader;
    [SerializeField] private float transitionTime = 1.2f;
    [SerializeField] private float diamondSize = 15f;

    private Vector3 oriPosition;
    private Quaternion oriRotation;
    private Vector3 objPos;
    private Quaternion objRot;

    private bool isClose = false;
    private bool load = false;
    private bool isTransitioning = false;

    private Material _transitionMat;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (toChrage != null)
        {
            sceneName = toChrage.name;
        }
        else
        {
            sceneName = "";
        }
    }
#endif

    void Start()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        oriPosition = _camera.transform.position;
        oriRotation = _camera.transform.rotation;

        SetupTransitionCanvas();
    }

    void SetupTransitionCanvas()
    {
        if (transitionShader == null)
        {
            transitionShader = Shader.Find("UI/DiamondTransition");
        }

        if (transitionShader != null)
        {
            _transitionMat = new Material(transitionShader);
            _transitionMat.SetFloat("_Progress", 0f);
            _transitionMat.SetFloat("_GridSize", diamondSize);

            // SHADER 
            GameObject canvasObj = new GameObject("TransitionCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasObj.AddComponent<CanvasScaler>();

            GameObject imageObj = new GameObject("TransitionImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            RawImage rawImage = imageObj.AddComponent<RawImage>();
            rawImage.material = _transitionMat;

            RectTransform rect = rawImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }
    }

    void OnMouseDown()
    {
        if (!isClose && !isTransitioning)
        {
            Vector3 moveTowardsCamera = (oriPosition - transform.position).normalized;

            objPos = transform.position + (moveTowardsCamera * _closeDistance) + new Vector3(offsetX, offsetY, offsetZ);

            objRot = Quaternion.LookRotation(transform.position - objPos);
            isClose = true;

            Invoke("SceneLoader", 0.2f);
        }
    }

    void SceneLoader()
    {
        load = true;
    }

    void Update()
    {
        if (isClose)
        {
            MoveCamera(objPos, objRot);

            if (!isTransitioning)
            {
                if (Input.GetKeyDown(_keyCode) || Input.GetMouseButtonDown(1))
                {
                    isClose = false;
                    load = false;
                    CancelInvoke("SceneLoader");
                }
                else if (Input.GetMouseButtonDown(0) && load)
                {
                    LoadNextScene();
                }
            }
        }
        else
        {
            MoveCamera(oriPosition, oriRotation);
        }
    }

    void MoveCamera(Vector3 finalPosition, Quaternion finalRotation)
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, finalPosition, Time.deltaTime * moveSpeed);
        _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, finalRotation, Time.deltaTime * moveSpeed);
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (_transitionMat != null)
            {
                StartCoroutine(FadeAndLoad());
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            Debug.LogError("JULIA ESTA A 42 KM!!! " + gameObject.name);
        }
    }

    private IEnumerator FadeAndLoad()
    {
        isTransitioning = true;
        float elapsed = 0f;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionTime);
            _transitionMat.SetFloat("_Progress", progress);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}