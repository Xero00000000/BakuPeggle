using UnityEngine;

public class UIPegLauncher : MonoBehaviour
{
    public Vector3 mousePos;
    [SerializeField] float minAngle;
    [SerializeField] float maxAngle;
    [SerializeField] float snapAngle;
    [SerializeField] private float angleOffset = 90f; // Offset de rotación
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform imageToRotate;

    void Update()
    {
        if (imageToRotate == null) return;

        mousePos = Input.mousePosition;
        Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
        Vector3 launcherScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, imageToRotate.position);
        Vector2 direction = mousePos - launcherScreenPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float clampedAngle = Mathf.Clamp(angle, minAngle, maxAngle);

        // Se le suma el offset a la rotación final
        imageToRotate.rotation = Quaternion.Euler(0, 0, clampedAngle + angleOffset);
    }
}