using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Imagen de la barra de vida (debe tener Image Type = Filled)")]
    [SerializeField] private Image healthFillImage;

    [Tooltip("Texto numérico opcional para mostrar los valores (ej. 100/100)")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Configuración de Animación")]
    [Tooltip("Velocidad con la que se reduce/llena la barra de vida")]
    [SerializeField] private float fillLerpSpeed = 8f;

    [Header("Dirección de la barra")]
    [SerializeField] private bool drainsRightToLeft = true;

    [Header("Shake con poca vida")]
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float shakeIntensity = 4f;
    [SerializeField] private float shakeSpeed = 40f;

    private int targetHP;
    private int maxHPCached;
    private float displayedHP;      // animated value, can go below 0
    private bool hasValue;
    private int lastShownHP = int.MinValue;
    private bool isLow;
    private RectTransform textRect;
    private Vector2 textBasePos;

    private void Awake()
    {
        if (healthFillImage != null)
        {
            healthFillImage.type = Image.Type.Filled;
            healthFillImage.fillMethod = Image.FillMethod.Horizontal;

            // Origin Left = the bar shrinks toward the left = drains right -> left
            healthFillImage.fillOrigin = drainsRightToLeft
                ? (int)Image.OriginHorizontal.Left
                : (int)Image.OriginHorizontal.Right;

            healthFillImage.fillAmount = 1f;
        }

        if (healthText != null)
        {
            textRect = healthText.rectTransform;
            textBasePos = textRect.anchoredPosition;
            healthText.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (hasValue)
        {
            // unscaled so it still animates while Time.timeScale = 0 (ability / turn notice)
            displayedHP = Mathf.Lerp(displayedHP, targetHP, Time.unscaledDeltaTime * fillLerpSpeed);
            if (Mathf.Abs(displayedHP - targetHP) < 0.5f) displayedHP = targetHP;

            if (healthFillImage != null && maxHPCached > 0)
                healthFillImage.fillAmount = Mathf.Clamp01(displayedHP / maxHPCached);   // the bar stops at empty

            UpdateHealthText();
        }

        if (isLow && textRect != null && healthText.gameObject.activeInHierarchy)
        {
            float t = Time.unscaledTime * shakeSpeed;
            Vector2 offset = new Vector2(
                Mathf.PerlinNoise(t, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * shakeIntensity;

            textRect.anchoredPosition = textBasePos + offset;
        }
    }

    public void UpdateHealth(int currentHP, int maxHP)
    {
        // No clamp: negative values are shown as they are
        if (!hasValue)
        {
            displayedHP = currentHP;
            hasValue = true;
        }

        targetHP = currentHP;
        maxHPCached = maxHP;

        isLow = maxHP > 0 && currentHP > 0 && (float)currentHP / maxHP <= lowHealthThreshold;
        if (!isLow) ResetTextPosition();
    }

    private void UpdateHealthText()
    {
        if (healthText == null || maxHPCached <= 0) return;

        int shown = Mathf.RoundToInt(displayedHP);

        if (shown == lastShownHP) return;   // avoid rebuilding the text every frame
        lastShownHP = shown;
        healthText.text = $"{shown}/{maxHPCached}";
    }

    private void ResetTextPosition()
    {
        if (textRect != null) textRect.anchoredPosition = textBasePos;
    }

    private void OnDisable()
    {
        ResetTextPosition();
    }

    public void ShowHealthText()
    {
        if (healthText != null)
        {
            healthText.gameObject.SetActive(true);
        }
    }

    public void HideHealthText()
    {
        if (healthText != null)
        {
            healthText.gameObject.SetActive(false);
        }
    }
}