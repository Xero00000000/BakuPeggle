using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.VFX; // Necesario para usar VisualEffect

public class FeedbackBalls : MonoBehaviour
{
    [SerializeField] private bool playerPeg;

    [SerializeField] private Renderer pegRenderer;
    private Collider2D pegCollider;
    [SerializeField] private Image image;

    [Header("Configuración de Tipos / Colores")]
    [SerializeField] private Material[] types;
    [SerializeField] private bool useRandomType = true;
    [SerializeField] private int currentType;

    [SerializeField] private PointsEventChannel _addPoints;

    [Header("Efecto de VFX")]
    [Tooltip("Asigna aquí un Prefab que contenga un componente VisualEffect.")]
    [SerializeField] private VisualEffect impactVFXPrefab;
    [Tooltip("Nombre del parámetro Exposed Color definido en el VFX Graph.")]
    [SerializeField] private string vfxColorPropertyName = "Color";

    [Header("Configuración de Sonido & Combo Pitch")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float basePitch = 1.0f;
    [SerializeField] private float pitchStep = 0.05f;
    [SerializeField] private float maxPitch = 2.0f;
    [SerializeField] private float comboResetTime = 2.0f;
    [SerializeField] private bool resetByTime = false;

    [Header("Post Processing Feedback (Global Volume)")]
    [Tooltip("Referencia al Global Volume de la escena (si se deja vacío, se buscará en Awake).")]
    [SerializeField] private Volume globalVolume;
    [Tooltip("Incremento de Aberración Cromática por golpe.")]
    [SerializeField] private float chromaticStep = 0.15f;
    [Tooltip("Límite máximo de Aberración Cromática.")]
    [SerializeField] private float maxChromaticAberration = 0.8f;

    [Header("Hit")]
    [SerializeField] private GameObject pegg;
    [SerializeField] private GameObject peggBg;

    [Header("Pop Destroy")]
    [SerializeField] private float popScaleFactor = 1.35f;
    [SerializeField] private float popDuration = 0.06f;
    [SerializeField] private float shrinkDuration = 0.12f;

    [Header("Hit Flash")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float flashDuration = 0.05f;

    [Header("Ghost Outline Effect")]
    [SerializeField] private bool enableGhostOutline = true;
    [SerializeField] private float ghostMaxScaleFactor = 1.8f;
    [SerializeField] private float ghostFadeDuration = 0.15f;
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.6f);

    [Header("Camera Shake")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float shakeIntensity = 0.08f;
    [SerializeField] private float shakeDuration = 0.08f;

    [Header("Texto Flotante")]
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private int basePoints = 100;
    public Color TextColor => GetComboTextColor();
    public int HitStreak => hitStreak;

    [Tooltip("Color del texto para cada tipo (mismo orden que 'types'). Si está vacío, se usa el color del material.")]
    [SerializeField] private Color[] typeTextColors;
    public int PegType => currentType;

    [Tooltip("Brillo (Value en HSV) del primer golpe del combo. 1 = brillo máximo.")]
    [SerializeField, Range(0.1f, 1f)] private float startBrightness = 0.55f;
    [Tooltip("Cantidad de golpes de combo para llegar al brillo máximo.")]
    [SerializeField] private int hitsToMaxBrightness = 8;
    [Tooltip("Mezcla máxima de blanco en el combo más alto (0 = nunca se vuelve blanco).")]
    [SerializeField, Range(0f, 0.5f)] private float maxWhiteMix = 0.1f;

    private static int playerStreak = 0;
    private static int enemyStreak = 0;
    private static float playerLastHitTime = 0f;
    private static float enemyLastHitTime = 0f;

    // Streak of this peg's own side
    private int hitStreak => playerPeg ? playerStreak : enemyStreak;

    private static ChromaticAberration chromaticComponent;

    private Camera mainCamera;
    private bool isHit = false;

    private void Awake()
    {
        mainCamera = Camera.main;

        SetupVolumeReference();

        if (pegRenderer == null) pegRenderer = GetComponent<Renderer>();
        pegCollider = GetComponent<Collider2D>();

        if (types != null && types.Length > 0)
        {
            if (useRandomType)
            {
                currentType = Random.Range(0, types.Length);
            }

            if (pegRenderer != null && types[currentType] != null)
            {
                pegRenderer.material = new Material(types[currentType]);

                switch (currentType)
                {
                    case 0:
                        if (image != null) image.color = Color.red;
                        break;
                    case 1:
                        if (image != null) image.color = Color.green;
                        break;
                    case 2:
                        if (image != null) image.color = Color.blue;
                        break;
                }
            }
        }
    }

    private void SetupVolumeReference()
    {
        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<Volume>();
        }

        if (globalVolume != null && globalVolume.profile != null)
        {
            if (chromaticComponent == null)
            {
                globalVolume.profile.TryGet(out chromaticComponent);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isHit) return;
        isHit = true;

        UpdateComboState();   // before the event, so the UI reads the current streak

        if (_addPoints != null)
        {
            _addPoints.Raise(this, playerPeg, currentType);
        }

        PlayEscalatingHitSound();
        VolumeApllications();
        StartParticles(collision);

        if (pegCollider != null) pegCollider.enabled = false;

        if (enableGhostOutline)
        {
            StartCoroutine(CreateGhostOutline());
        }

        if (enableCameraShake)
        {
            StartCoroutine(ApplyCameraShake());
        }

        StartCoroutine(AnimatePopAndDestroy());
    }

    private IEnumerator CreateGhostOutline()
    {
        if (pegRenderer == null) yield break;

        GameObject ghostObj = new GameObject("PegGhostOutline");
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;
        ghostObj.transform.localScale = transform.localScale;

        if (pegRenderer is SpriteRenderer originalSprite)
        {
            SpriteRenderer ghostSprite = ghostObj.AddComponent<SpriteRenderer>();
            ghostSprite.sprite = originalSprite.sprite;
            ghostSprite.color = ghostColor;
            ghostSprite.sortingLayerID = originalSprite.sortingLayerID;
            ghostSprite.sortingOrder = originalSprite.sortingOrder - 1;
        }
        else
        {
            MeshFilter originalFilter = GetComponent<MeshFilter>();
            if (originalFilter != null)
            {
                MeshFilter ghostFilter = ghostObj.AddComponent<MeshFilter>();
                ghostFilter.mesh = originalFilter.mesh;

                MeshRenderer ghostMeshRenderer = ghostObj.AddComponent<MeshRenderer>();
                ghostMeshRenderer.material = new Material(pegRenderer.material);
                if (ghostMeshRenderer.material.HasProperty("_Color"))
                {
                    ghostMeshRenderer.material.color = ghostColor;
                }
            }
        }

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * ghostMaxScaleFactor;

        float elapsed = 0f;
        Renderer ghostRen = ghostObj.GetComponent<Renderer>();

        while (elapsed < ghostFadeDuration)
        {
            float t = elapsed / ghostFadeDuration;

            ghostObj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (ghostRen != null && ghostRen.material.HasProperty("_Color"))
            {
                Color c = ghostRen.material.color;
                c.a = Mathf.Lerp(ghostColor.a, 0f, t);
                ghostRen.material.color = c;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Destroy(ghostObj);
    }

    private IEnumerator AnimatePopAndDestroy()
    {
        Color originalColor = Color.white;
        bool hasColorProp = pegRenderer != null && pegRenderer.material.HasProperty("_Color");

        if (hasColorProp)
        {
            originalColor = pegRenderer.material.color;
            pegRenderer.material.color = hitFlashColor;
        }

        Vector3 originalScale = transform.localScale;
        Vector3 targetPopScale = originalScale * popScaleFactor;

        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetPopScale, elapsed / popDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (hasColorProp)
        {
            pegRenderer.material.color = originalColor;
        }

        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(targetPopScale, Vector3.zero, elapsed / shrinkDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (pegRenderer != null) pegRenderer.enabled = false;

        if (pegg != null) Destroy(pegg);
        if (peggBg != null) Destroy(peggBg);

        Destroy(gameObject);
    }

    private IEnumerator ApplyCameraShake()
    {
        if (mainCamera == null) yield break;

        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity;
            mainCamera.transform.localPosition = originalPos + new Vector3(randomOffset.x, randomOffset.y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    

    private Color GetBaseTypeColor()
    {
        // 1) Color explícito por tipo
        if (typeTextColors != null && currentType < typeTextColors.Length)
            return typeTextColors[currentType];

        // 2) Color del material del tipo
        if (types != null && currentType < types.Length && types[currentType] != null
            && types[currentType].HasProperty("_Color"))
            return types[currentType].color;

        // 3) Fallback al mismo esquema que usas para la imagen
        switch (currentType)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            default: return Color.white;
        }
    }

    private Color GetComboTextColor()
    {
        Color baseColor = GetBaseTypeColor();

        // 0 en el primer golpe, 1 al llegar a hitsToMaxBrightness
        float comboSteps = Mathf.Max(0, hitStreak - 1);
        float t = Mathf.Clamp01(comboSteps / Mathf.Max(1, hitsToMaxBrightness - 1));

        // Mantener hue y saturación; solo subir el brillo (Value)
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        float newV = Mathf.Lerp(v * startBrightness, v, t);

        Color result = Color.HSVToRGB(h, s, newV);

        // Un toque mínimo de blanco al final para dar sensación de "incandescente"
        result = Color.Lerp(result, Color.white, t * maxWhiteMix);

        result.a = baseColor.a;
        return result;
    }

    public static void ResetCombos()
    {
        playerStreak = 0;
        enemyStreak = 0;
        playerLastHitTime = 0f;
        enemyLastHitTime = 0f;
        if (chromaticComponent != null) chromaticComponent.intensity.value = 0f;
    }

    private void UpdateComboState()
    {
        ref int streak = ref (playerPeg ? ref playerStreak : ref enemyStreak);
        ref float lastTime = ref (playerPeg ? ref playerLastHitTime : ref enemyLastHitTime);

        if (resetByTime && Time.time - lastTime > comboResetTime)   // <-- changed condition
        {
            streak = 0;
            if (playerPeg && chromaticComponent != null)
            {
                chromaticComponent.intensity.value = 0f;
            }
        }

        lastTime = Time.time;
        streak++;
    }

    private void PlayEscalatingHitSound()
    {
        if (hitSound == null) return;

        float calculatedPitch = basePitch + ((hitStreak - 1) * pitchStep);
        calculatedPitch = Mathf.Min(calculatedPitch, maxPitch);

        GameObject soundObj = new GameObject("TempHitAudio");
        soundObj.transform.position = transform.position;

        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = hitSound;
        audioSource.pitch = calculatedPitch;
        audioSource.Play();

        Destroy(soundObj, hitSound.length / calculatedPitch);
    }

    private void VolumeApllications()
    {
        if (chromaticComponent == null) return;

        chromaticComponent.intensity.overrideState = true;
        float currentIntensity = chromaticComponent.intensity.value;
        float newIntensity = Mathf.Min(currentIntensity + chromaticStep, maxChromaticAberration);

        chromaticComponent.intensity.value = newIntensity;
    }

    private void StartParticles(Collision2D collision)
    {
        if (impactVFXPrefab == null) return;

        Vector3 spawnPosition;
        if (collision != null && collision.contactCount > 0)
        {
            Vector2 contactPoint = collision.GetContact(0).point;
            spawnPosition = new Vector3(contactPoint.x, contactPoint.y, transform.position.z);
        }
        else
        {
            spawnPosition = transform.position;
        }

        VisualEffect vfxInstance = Instantiate(impactVFXPrefab, spawnPosition, Quaternion.identity);

        Color targetColor = Color.white;
        if (types != null && types.Length > currentType && types[currentType] != null && types[currentType].HasProperty("_Color"))
        {
            targetColor = types[currentType].color;
        }

        vfxInstance.SetVector4(vfxColorPropertyName, targetColor);
        vfxInstance.Play();

        Destroy(vfxInstance.gameObject, 2f);
    }
}