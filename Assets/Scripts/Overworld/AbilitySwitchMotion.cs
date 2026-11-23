using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySwitchMotion : MonoBehaviour
{
    [Serializable]
    public struct CreatureSlot
    {
        public string label;
        public ResizableItem itemReference; // Damage, Healing, Duplicate, Bomb
        public Color slotAccentColor;     // Color propio de esta criatura/habilidad
    }

    [Header("Objetos a Animar al Cambiar")]
    [SerializeField] private RectTransform barRectTransform;     // Tu objeto "Barra"
    [SerializeField] private RectTransform movingAccentRect;  // Opcional: HabilidadLine o Effect (1)
    [SerializeField] private GameObject optionalFlashObject;    // Opcional: Effect (1)

    [Header("Destello y Partículas por Criatura")]
    [SerializeField] private Image slotFlashImage;          // Imagen encima de la barra para el destello
    [SerializeField] private ParticleSystem slotParticles;  // Partículas cortas opcionales
    [SerializeField] private float flashPeakAlpha = 0.45f;
    [SerializeField] private float flashDuration = 0.18f;

    [Header("Configuración del Movimiento")]
    [SerializeField] private float punchScale = 1.07f;
    [SerializeField] private float motionDuration = 0.16f;
    [SerializeField] private float accentTravelX = 120f;

    [Header("Slots a Observar")]
    [SerializeField] private CreatureSlot[] slots;

    private int currentIndex = -1;
    private Vector3 baseScale;
    private Vector2 baseBarAnchoredPos;
    private Vector2 baseAccentAnchoredPos;
    private Coroutine activeMotionRoutine;
    private Coroutine activeFlashRoutine;

    void Awake()
    {
        if (barRectTransform != null)
        {
            baseScale = barRectTransform.localScale;
            baseBarAnchoredPos = barRectTransform.anchoredPosition;
        }

        if (movingAccentRect != null)
        {
            baseAccentAnchoredPos = movingAccentRect.anchoredPosition;
        }

        if (slotFlashImage != null)
        {
            Color c = slotFlashImage.color;
            c.a = 0f;
            slotFlashImage.color = c;
        }
    }

    void Start()
    {
        currentIndex = DetectSelectedIndex();
    }

    void Update()
    {
        int detected = DetectSelectedIndex();
        if (detected != -1 && detected != currentIndex)
        {
            currentIndex = detected;
            PlaySwitchMotion(currentIndex);
        }
    }

    private int DetectSelectedIndex()
    {
        int bestIndex = -1;
        float highestScale = -1f;

        for (int i = 0; i < slots.Length; i++)
        {
            ResizableItem item = slots[i].itemReference;
            if (item == null || !item.gameObject.activeInHierarchy) continue;

            float scaleSqr = item.transform.localScale.sqrMagnitude;
            if (scaleSqr > highestScale)
            {
                highestScale = scaleSqr;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public void PlaySwitchMotion(int slotIndex)
    {
        Color accentColor = Color.white;
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            accentColor = slots[slotIndex].slotAccentColor;
            accentColor.a = 1f;
        }

        TriggerSlotEffects(accentColor);

        if (activeMotionRoutine != null)
        {
            StopCoroutine(activeMotionRoutine);
        }

        activeMotionRoutine = StartCoroutine(AnimateSwitchRoutine());
    }

    private void TriggerSlotEffects(Color accentColor)
    {
        if (slotParticles != null)
        {
            var main = slotParticles.main;
            main.startColor = accentColor;
            slotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            slotParticles.Play();
        }

        if (slotFlashImage != null)
        {
            if (activeFlashRoutine != null)
            {
                StopCoroutine(activeFlashRoutine);
            }
            activeFlashRoutine = StartCoroutine(AnimateFlashRoutine(accentColor));
        }
    }

    private IEnumerator AnimateFlashRoutine(Color accentColor)
    {
        float halfDuration = Mathf.Max(0.01f, flashDuration * 0.5f);
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            Color c = accentColor;
            c.a = Mathf.Lerp(0f, flashPeakAlpha, t);
            slotFlashImage.color = c;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            Color c = accentColor;
            c.a = Mathf.Lerp(flashPeakAlpha, 0f, t);
            slotFlashImage.color = c;
            yield return null;
        }

        Color finalColor = accentColor;
        finalColor.a = 0f;
        slotFlashImage.color = finalColor;
        activeFlashRoutine = null;
    }

    private IEnumerator AnimateSwitchRoutine()
    {
        if (optionalFlashObject != null)
        {
            optionalFlashObject.SetActive(false);
            optionalFlashObject.SetActive(true);
        }

        float elapsed = 0f;
        float halfDuration = Mathf.Max(0.01f, motionDuration * 0.5f);

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            if (barRectTransform != null)
            {
                float currentScaleMult = Mathf.Lerp(1f, punchScale, smooth);
                barRectTransform.localScale = baseScale * currentScaleMult;
            }

            if (movingAccentRect != null)
            {
                movingAccentRect.anchoredPosition = baseAccentAnchoredPos + new Vector2(accentTravelX * smooth, 0f);
            }

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            if (barRectTransform != null)
            {
                float currentScaleMult = Mathf.Lerp(punchScale, 1f, smooth);
                barRectTransform.localScale = baseScale * currentScaleMult;
            }

            if (movingAccentRect != null)
            {
                movingAccentRect.anchoredPosition = baseAccentAnchoredPos + new Vector2(accentTravelX * (1f - smooth), 0f);
            }

            yield return null;
        }

        ResetTransforms();
        activeMotionRoutine = null;
    }

    private void ResetTransforms()
    {
        if (barRectTransform != null)
        {
            barRectTransform.localScale = baseScale;
            barRectTransform.anchoredPosition = baseBarAnchoredPos;
        }

        if (movingAccentRect != null)
        {
            movingAccentRect.anchoredPosition = baseAccentAnchoredPos;
        }
    }
}