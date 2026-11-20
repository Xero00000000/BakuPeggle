using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageEffectManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("UI Image a pantalla completa que usará el Shader Custom/Damage")]
    [SerializeField] private Image fullScreenDamageImage;

    [Tooltip("Material que tiene asignado el Shader Custom/Damage")]
    [SerializeField] private Material lightningMaterial;

    [Header("Cámara (Screen Shake)")]
    [Tooltip("Cámara principal que temblará al recibir daño. Si se deja nulo, buscará Camera.main automáticamente.")]
    [SerializeField] private Camera mainCamera;

    [Header("Configuración del Shader")]
    [SerializeField] private string progressPropertyName = "_Progress";
    [SerializeField] private float flashDuration = 0.4f;
    [SerializeField] private float maxIntensity = 1.0f;

    [Header("Configuración del Camera Shake")]
    [Tooltip("Intensidad del movimiento de la cámara (un valor entre 0.05 y 0.2 es un temblor leve)")]
    [SerializeField] private float shakeMagnitude = 0.12f;

    [Header("Per-ability overrides")]
    [Tooltip("Color property on the Custom/Damage shader (_Color, _Tint...). Leave empty if it has none.")]
    [SerializeField] private string tintPropertyName = "_Color";
    private Color defaultTint = Color.white;
    private bool hasTint;

    private Material instantiatedMaterial;
    private Coroutine lightningCoroutine;
    private Coroutine shakeCoroutine;
    private Vector3 originalCameraPosition;

    private void Awake()
    {
        // Asignar cámara principal si no se configuró en el Inspector
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }

        if (lightningMaterial != null && fullScreenDamageImage != null)
        {
            // Instanciar el material para no modificar el archivo original en el proyecto
            instantiatedMaterial = new Material(lightningMaterial);
            fullScreenDamageImage.material = instantiatedMaterial;
            hasTint = !string.IsNullOrEmpty(tintPropertyName) && instantiatedMaterial.HasProperty(tintPropertyName);
            if (hasTint) defaultTint = instantiatedMaterial.GetColor(tintPropertyName);

            // Iniciar apagado y ocultar la imagen
            instantiatedMaterial.SetFloat(progressPropertyName, 0f);
            fullScreenDamageImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Falta asignar 'fullScreenDamageImage' o 'lightningMaterial' en el Inspector.");
        }
    }

    /// <summary>
    /// Dispara el destello de rayos a pantalla completa y el temblor suave de cámara.
    /// </summary>
    public void TriggerDamageEffect()
    {
        // 1. Activar animación del Shader
        if (instantiatedMaterial != null && fullScreenDamageImage != null)
        {
            if (lightningCoroutine != null)
                StopCoroutine(lightningCoroutine);

            lightningCoroutine = StartCoroutine(LightningRoutine());
        }

        // 2. Activar Screen Shake en la Cámara
        if (mainCamera != null)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                mainCamera.transform.localPosition = originalCameraPosition; // Restaurar posición previa si se interrumpe
            }

            shakeCoroutine = StartCoroutine(CameraShakeRoutine());
        }
    }

    private IEnumerator LightningRoutine()
    {
        fullScreenDamageImage.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / flashDuration;

            // Pulso suave: sube a maxIntensity y vuelve a 0
            float progressValue = Mathf.Sin(normalizedTime * Mathf.PI) * maxIntensity;

            instantiatedMaterial.SetFloat(progressPropertyName, progressValue);
            yield return null;
        }

        instantiatedMaterial.SetFloat(progressPropertyName, 0f);
        fullScreenDamageImage.gameObject.SetActive(false);
    }

    private IEnumerator CameraShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;

            // Suavizar el impacto a medida que pasa el tiempo
            float dampingFactor = 1f - (elapsed / flashDuration);

            // Generar desplazamiento aleatorio en X e Y
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude * dampingFactor;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude * dampingFactor;

            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        // Asegurar que la cámara regrese exactamente a su posición original
        mainCamera.transform.localPosition = originalCameraPosition;
    }

    public void TriggerCustom(Color tint, float intensity, float duration, float shake)
    {
        if (instantiatedMaterial != null && fullScreenDamageImage != null && intensity > 0f)
        {
            if (lightningCoroutine != null) StopCoroutine(lightningCoroutine);
            lightningCoroutine = StartCoroutine(LightningCustom(tint, intensity, duration));
        }

        if (mainCamera != null && shake > 0f)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                mainCamera.transform.localPosition = originalCameraPosition;
            }
            shakeCoroutine = StartCoroutine(ShakeCustom(shake, duration));
        }
    }

    private IEnumerator LightningCustom(Color tint, float intensity, float duration)
    {
        if (hasTint) instantiatedMaterial.SetColor(tintPropertyName, tint);
        fullScreenDamageImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI) * intensity * maxIntensity;
            instantiatedMaterial.SetFloat(progressPropertyName, p);
            yield return null;
        }

        instantiatedMaterial.SetFloat(progressPropertyName, 0f);
        if (hasTint) instantiatedMaterial.SetColor(tintPropertyName, defaultTint);
        fullScreenDamageImage.gameObject.SetActive(false);
    }

    private IEnumerator ShakeCustom(float magnitude, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float damp = 1f - Mathf.Clamp01(elapsed / duration);
            Vector2 r = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * magnitude * damp;
            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(r.x, r.y, 0f);
            yield return null;
        }
        mainCamera.transform.localPosition = originalCameraPosition;
    }

    private void OnDestroy()
    {
        if (instantiatedMaterial != null)
        {
            Destroy(instantiatedMaterial);
        }
    }
}