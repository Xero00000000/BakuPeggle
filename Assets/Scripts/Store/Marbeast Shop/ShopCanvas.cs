using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShopCanvas : MonoBehaviour
{
    public enum CameraState { Origin, Step1_Target, Step2_Canvas }

    [Header("Camera and points")]
    public Camera mainCamera;
    public Transform targetStep1;          // optional approach point
    public Transform targetStep2;          // where the camera sits to view the shop
    public Canvas canvasToEnable;          // the store canvas

    [Header("Movement")]
    public float moveSpeed = 5f;
    public Vector3 offsetStep1 = Vector3.zero;
    public Vector3 offsetStep2 = Vector3.zero;

    [Header("Look at counter")]
    public float turnAngle = -60f;   // 180 = look behind. Try 90 / -90 for the sides

    [Header("Transition (use the SAME material as GachaCanvas)")]
    public Material transitionMaterial;
    public float transitionDuration = 0.6f;

    [Header("Open canvas")]
    public bool autoOpen = false;          // true = canvas opens by itself
    public float autoOpenDelay = 1f;       // time at Step 1 before it opens
    private float step1Time;

    [Header("Bobbing")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 2f;
    public float bobbingAmountY = 0.05f;
    public float bobbingAmountX = 0.03f;

    [Header("Controls")]
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    [Header("Exit")]
    public float reverseDelay = 0.6f;   // time spent turning back before travelling home
    private bool exiting;

    [Header("Step 2 pose")]
    public bool useTarget2Rotation = false;   // true = copy target 2's rotation, false = keep the turned rotation from Step 1

    private CameraState currentState = CameraState.Origin;
    private Vector3 oriPosition;
    private Quaternion oriRotation;
    private float bobbingTimer;
    private Coroutine transitionCoroutine;
    private Vector3 holdPosition;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        oriPosition = mainCamera.transform.position;
        oriRotation = mainCamera.transform.rotation;

        if (canvasToEnable != null) canvasToEnable.gameObject.SetActive(false);
        SetupOverlayCanvas();
    }

    void SetupOverlayCanvas()
    {
        if (transitionMaterial == null) return;
        if (GameObject.Find("TransitionOverlayCanvas") != null) return; // already created by the other script

        GameObject canvasObj = new GameObject("TransitionOverlayCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasObj.AddComponent<CanvasScaler>();

        GameObject imgObj = new GameObject("TransitionRawImage");
        imgObj.transform.SetParent(canvasObj.transform, false);
        RawImage overlay = imgObj.AddComponent<RawImage>();
        overlay.material = transitionMaterial;
        overlay.raycastTarget = false;

        RectTransform rect = overlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        transitionMaterial.SetFloat("_Progress", 0f);
    }

    void OnMouseDown()
    {
        if (exiting) return;
        if (!CameraLock.TryAcquire(this)) return;   // the duplicate line is gone

        if (currentState == CameraState.Origin)
        {
            oriPosition = mainCamera.transform.position;
            oriRotation = mainCamera.transform.rotation;
            bobbingTimer = 0f;

            ChangeState(targetStep1 != null ? CameraState.Step1_Target : CameraState.Step2_Canvas);
        }
    }

    void Update()
    {
        if (!CameraLock.IsOwner(this)) return;

        if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
            RevertState();

        if (currentState == CameraState.Step1_Target && !exiting)
        {
            float waited = Time.time - step1Time;

            bool clicked = Input.GetMouseButtonDown(0) && waited > 0.4f;   // 0.4s guard so the first click doesn't count twice
            bool auto = autoOpen && waited > autoOpenDelay;

            if (clicked || auto)
                ChangeState(CameraState.Step2_Canvas);
        }

        Vector3 targetPos = oriPosition;
        Quaternion targetRot = oriRotation;

        switch (currentState)
        {
            case CameraState.Step1_Target:
            case CameraState.Step2_Canvas:
                GetShopPose(out targetPos, out targetRot);
                break;
        }

        if (enableBobbing && currentState == CameraState.Step1_Target)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            float offsetY = Mathf.Sin(bobbingTimer) * bobbingAmountY;
            float offsetX = Mathf.Cos(bobbingTimer * 0.5f) * bobbingAmountX;
            targetPos += mainCamera.transform.up * offsetY + mainCamera.transform.right * offsetX;
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * moveSpeed);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * moveSpeed);

        // Back at origin and settled -> give the camera back
        if (currentState == CameraState.Origin &&
            (mainCamera.transform.position - oriPosition).sqrMagnitude < 0.0001f &&
            Quaternion.Angle(mainCamera.transform.rotation, oriRotation) < 0.5f)
        {
            mainCamera.transform.SetPositionAndRotation(oriPosition, oriRotation);
            CameraLock.Release(this);
        }
    }

    void ChangeState(CameraState newState)
    {
        currentState = newState;

        if (newState == CameraState.Step1_Target) step1Time = Time.time;

        if (canvasToEnable != null)
            canvasToEnable.gameObject.SetActive(currentState == CameraState.Step2_Canvas);

        TriggerShaderTransition();
    }

    void RevertState()
    {
        if (exiting) return;

        if (currentState == CameraState.Step2_Canvas)
        {
            if (targetStep1 != null) StartCoroutine(ExitSequence());
            else ChangeState(CameraState.Origin);
        }
        else if (currentState == CameraState.Step1_Target)
        {
            ChangeState(CameraState.Origin);
        }
    }

    IEnumerator ExitSequence()
    {
        exiting = true;

        ChangeState(CameraState.Step1_Target);      // 1) turn back from the counter, close the canvas
        yield return new WaitForSeconds(reverseDelay);

        ChangeState(CameraState.Origin);            // 2) travel back to where the camera started
        exiting = false;
    }

    void TriggerShaderTransition()
    {
        if (transitionMaterial == null) return;
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimateTransitionShader());
    }

    IEnumerator AnimateTransitionShader()
    {
        float elapsed = 0f;
        float half = transitionDuration / 2f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed <= half
                ? Mathf.Clamp01(elapsed / half)
                : Mathf.Clamp01((transitionDuration - elapsed) / half);
            transitionMaterial.SetFloat("_Progress", p);
            yield return null;
        }
        transitionMaterial.SetFloat("_Progress", 0f);
    }



    void GetShopPose(out Vector3 position, out Quaternion rotation)
    {
        // Step 1 pose: target 1 position + turn from the original facing
        Vector3 pos1 = targetStep1 != null
            ? targetStep1.position + targetStep1.TransformDirection(offsetStep1)
            : oriPosition;

        float yaw = oriRotation.eulerAngles.y + turnAngle;
        float pitch = oriRotation.eulerAngles.x;
        Quaternion rot1 = Quaternion.Euler(pitch, yaw, 0f);

        if (currentState == CameraState.Step2_Canvas && targetStep2 != null)
        {
            // Step 2 pose: move to target 2
            position = targetStep2.position + targetStep2.TransformDirection(offsetStep2);
            rotation = useTarget2Rotation ? targetStep2.rotation : rot1;
        }
        else
        {
            position = pos1;
            rotation = rot1;
        }
    }

    void OnDestroy()
    {
        CameraLock.Release(this);
    }
}