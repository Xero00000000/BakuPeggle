using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI; // Necesario para Post-Processing / URP Volume

public class FeedbackBalls : MonoBehaviour
{
    [SerializeField] private bool playerPeg;

    [SerializeField] private Renderer pegRenderer;
    private Collider2D pegCollider;
    [SerializeField] private Image image;

    [Header("Configuración de Tipos / Colores")]
    [SerializeField] private Material[] types;
    //[SerializeField] private Color[] types;
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
    [Tooltip("Referencia al Global Volume de la escena.")]
    [SerializeField] private Volume globalVolume;
    [Tooltip("Incremento de Aberración Cromática por golpe.")]
    [SerializeField] private float chromaticStep = 0.15f;
    [Tooltip("Límite máximo de Aberración Cromática.")]
    [SerializeField] private float maxChromaticAberration = 0.8f;

    // Variables estáticas compartidas para el combo
    private static int hitStreak = 0;
    private static float lastHitTime = 0f;
    private static ChromaticAberration chromaticComponent;

    public void Start()
    {
        pegRenderer = GetComponent<Renderer>();
        pegCollider = GetComponent<Collider2D>();

        // 1. Asignar tipo/material aleatorio o manual
        if (types != null && types.Length > 0)
        {
            if (useRandomType)
            {
                currentType = Random.Range(0, types.Length);
            }

            if (pegRenderer != null && types[currentType] != null)
            {
                pegRenderer.material = types[currentType];

                switch(currentType)
                {
                    case 0:
                        image.color = Color.red;
                        break;
                    case 1:
                        image.color = Color.green;
                        break;
                    case 2:
                        image.color = Color.blue;
                        break;

                }
            }
        }

        // 2. Obtener o buscar el Global Volume para la Aberración Cromática
        SetupVolumeReference();
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
        if (_addPoints != null)
        {
            _addPoints.Raise(this, playerPeg, currentType);
        }

        // Verificar el estado del tiempo del combo
        UpdateComboState();

        // Reproducir audio con pitch acumulativo
        PlayEscalatingHitSound();

        // Aumentar la aberración cromática en el Post-Processing
        VolumeApllications();

        // Generar partículas en coordenadas globales
        SpawnImpactParticles(collision);

        // Ocultar y desactivar colisión
        if (pegRenderer != null) pegRenderer.enabled = false;
        if (pegCollider != null) pegCollider.enabled = false;

        Destroy(gameObject);
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

        // Posición limpia en World Space
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
        if (types != null && types.Length > currentType && types[currentType] != null)
        {
            targetColor = types[currentType].color;
        }

        var mainModule = particles.main;
        mainModule.startColor = targetColor;

        particles.Play();
        Destroy(particles.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);
    }
}