using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GachaCanvas : MonoBehaviour
{
    public enum CameraState { Origin, Step1_Target, Step2_Canvas }

    [Header("Referencias de Cámara y Puntos")]
    public Camera mainCamera;
    public Transform targetStep1;
    public Transform targetStep2;
    public Canvas canvasToEnable;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public Vector3 offsetStep1 = Vector3.zero;
    public Vector3 offsetStep2 = Vector3.zero;

    [Header("Efecto Transición (Shader HLSL)")]
    public Material transitionMaterial;
    public float transitionDuration = 0.6f;

    [Header("Efecto Bobbing (Bamboleo)")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 2f;
    public float bobbingAmountY = 0.05f;
    public float bobbingAmountX = 0.03f;

    [Header("Controles")]
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    private CameraState currentState = CameraState.Origin;
    private Vector3 oriPosition;
    private Quaternion oriRotation;
    private float bobbingTimer = 0f;
    private Coroutine transitionCoroutine;

    private RawImage overlayImage;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        oriPosition = mainCamera.transform.position;
        oriRotation = mainCamera.transform.rotation;

        if (canvasToEnable != null)
            canvasToEnable.gameObject.SetActive(false);
        SetupOverlayCanvas();
    }

    void SetupOverlayCanvas()
    {
        if (transitionMaterial == null) return;

        GameObject canvasObj = new GameObject("TransitionOverlayCanvas");
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

    void OnMouseDown()
    {
        if (currentState == CameraState.Origin && targetStep1 != null)
        {
            ChangeState(CameraState.Step1_Target);
        }
        else if (currentState == CameraState.Step1_Target && targetStep2 != null)
        {
            ChangeState(CameraState.Step2_Canvas);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
        {
            RevertState();
        }

        Vector3 targetPos = oriPosition;
        Quaternion targetRot = oriRotation;

        switch (currentState)
        {
            case CameraState.Step1_Target:
                CalculateTargetTransforms(targetStep1, offsetStep1, out targetPos, out targetRot);
                break;

            case CameraState.Step2_Canvas:
                CalculateTargetTransforms(targetStep2, offsetStep2, out targetPos, out targetRot);
                break;
        }

        if (enableBobbing && currentState != CameraState.Origin)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            float offsetY = Mathf.Sin(bobbingTimer) * bobbingAmountY;
            float offsetX = Mathf.Cos(bobbingTimer * 0.5f) * bobbingAmountX;
            targetPos += mainCamera.transform.up * offsetY + mainCamera.transform.right * offsetX;
        }

        MoveCamera(targetPos, targetRot);
    }

    void ChangeState(CameraState newState)
    {
        currentState = newState;

        if (canvasToEnable != null)
            canvasToEnable.gameObject.SetActive(currentState == CameraState.Step2_Canvas);

        TriggerShaderTransition();
    }

    void RevertState()
    {
        if (currentState == CameraState.Step2_Canvas)
            ChangeState(CameraState.Step1_Target);
        else if (currentState == CameraState.Step1_Target)
            ChangeState(CameraState.Origin);
    }

    void TriggerShaderTransition()
    {
        if (transitionMaterial == null) return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(AnimateTransitionShader());
    }

    private IEnumerator AnimateTransitionShader()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float halfDuration = transitionDuration / 2f;

            float progress = (elapsed <= halfDuration)
                ? Mathf.Clamp01(elapsed / halfDuration)
                : Mathf.Clamp01((transitionDuration - elapsed) / halfDuration);

            transitionMaterial.SetFloat("_Progress", progress);
            yield return null;
        }

        transitionMaterial.SetFloat("_Progress", 0f);
    }

    void CalculateTargetTransforms(Transform target, Vector3 offset, out Vector3 position, out Quaternion rotation)
    {
        position = target.position + target.TransformDirection(offset);
        rotation = Quaternion.LookRotation(target.position - position);
    }

    void MoveCamera(Vector3 finalPosition, Quaternion finalRotation)
    {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, finalPosition, Time.deltaTime * moveSpeed);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, finalRotation, Time.deltaTime * moveSpeed);
    }
}