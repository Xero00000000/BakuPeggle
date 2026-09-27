using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

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

    [Header("Efecto de Partículas")]
    [Tooltip("Asigna aquí un Prefab de Particle System.")]
    [SerializeField] private ParticleSystem impactParticlesPrefab;

    [Header("Configuración de Sonido & Combo Pitch")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float basePitch = 1.0f;
    [SerializeField] private float pitchStep = 0.05f;
    [SerializeField] private float maxPitch = 2.0f;
    [SerializeField] private float comboResetTime = 2.0f;

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
    [Tooltip("Multiplicador de tamaño máximo antes de implosionar (ej: 1.35 = 35% más grande).")]
    [SerializeField] private float popScaleFactor = 1.35f;
    [Tooltip("Duración en segundos del aumento de tamaño.")]
    [SerializeField] private float popDuration = 0.06f;
    [Tooltip("Duración en segundos de la implosión (reducción a 0).")]
    [SerializeField] private float shrinkDuration = 0.12f;

    [Header("Hit Flash")]
    [Tooltip("Color del destello al momento del impacto.")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 1f, 1f, 0.8f);
    [Tooltip("Duración en segundos del destello blanco.")]
    [SerializeField] private float flashDuration = 0.05f;

    [Header("Ghost Outline Effect")]
    [Tooltip("¿Activar la creación de un contorno fantasma que se expande?")]
    [SerializeField] private bool enableGhostOutline = true;
    [Tooltip("Escala máxima que alcanza la onda fantasma (ej: 1.8 = 80% más grande).")]
    [SerializeField] private float ghostMaxScaleFactor = 1.8f;
    [Tooltip("Duración de la expansión y desvanecimiento del contorno.")]
    [SerializeField] private float ghostFadeDuration = 0.15f;
    [Tooltip("Color tintado del borde fantasma.")]
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.6f);

    [Header("Camera Shake")]
    [Tooltip("¿Activar una leve sacudida de cámara al impactar?")]
    [SerializeField] private bool enableCameraShake = true;
    [Tooltip("Intensidad de la vibración de la cámara.")]
    [SerializeField] private float shakeIntensity = 0.08f;
    [Tooltip("Duración en segundos de la vibración.")]
    [SerializeField] private float shakeDuration = 0.08f;

    [Header("Hitstop / Freeze Frame")]
    [Tooltip("¿Activar la micro pausa de tiempo al recibir el golpe?")]
    [SerializeField] private bool enableHitstop = true;
    [Tooltip("Combo mínimo necesario para activar la pausa de tiempo.")]
    [SerializeField] private int minComboForHitstop = 1;
    [Tooltip("Duración en segundos reales del congelamiento de pantalla.")]
    [SerializeField] private float hitstopDuration = 0.03f;

    [Header("Texto Flotante")]
    [Tooltip("Prefab opcional con el script FloatingText para mostrar el puntaje.")]
    [SerializeField] private FloatingText floatingTextPrefab;
    [Tooltip("Puntos base que otorga este peg.")]
    [SerializeField] private int basePoints = 100;

    private static int hitStreak = 0;
    private static float lastHitTime = 0f;
    private static ChromaticAberration chromaticComponent;

    private Camera mainCamera;
    private bool isHit = false;

    private void Awake()
    {
        mainCamera = Camera.main;

        SetupVolumeReference();
    }

    public void Start()
    {
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

        if (_addPoints != null)
        {
            _addPoints.Raise(this, playerPeg, currentType);
        }

        UpdateComboState();
        PlayEscalatingHitSound();
        VolumeApllications();
        SpawnImpactParticles(collision);

        SpawnFloatingText();

        if (pegCollider != null) pegCollider.enabled = false;

        if (enableGhostOutline)
        {
            StartCoroutine(CreateGhostOutline());
        }

        if (enableHitstop && hitStreak >= minComboForHitstop)
        {
            StartCoroutine(ApplyHitstop());
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

    private IEnumerator ApplyHitstop()
    {
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(hitstopDuration);

        Time.timeScale = previousTimeScale;
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

    private void SpawnFloatingText()
    {
        if (floatingTextPrefab == null) return;

        int multiplier = Mathf.Max(1, hitStreak);
        int totalPoints = basePoints * multiplier;

        string text = hitStreak > 1 ? $"+{totalPoints} (x{hitStreak})" : $"+{totalPoints}";
        Color color = hitStreak >= 3 ? new Color(1f, 0.85f, 0f) : Color.white; // Dorado en combos altos

        Vector3 spawnPos = transform.position + Vector3.up * 0.3f;
        FloatingText ft = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
        ft.Setup(text, color);
    }

    private void UpdateComboState()
    {
        if (Time.time - lastHitTime > comboResetTime)
        {
            hitStreak = 0;
            if (chromaticComponent != null)
            {
                chromaticComponent.intensity.value = 0f;
            }
        }

        lastHitTime = Time.time;
        hitStreak++;
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

    private void SpawnImpactParticles(Collision2D collision)
    {
        if (impactParticlesPrefab == null) return;

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

        ParticleSystem particles = Instantiate(impactParticlesPrefab, spawnPosition, Quaternion.identity, null);

        Color targetColor = Color.white;
        if (types != null && types.Length > currentType && types[currentType] != null && types[currentType].HasProperty("_Color"))
        {
            targetColor = types[currentType].color;
        }

        var mainModule = particles.main;
        mainModule.startColor = targetColor;

        particles.Play();
        Destroy(particles.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);
    }
}