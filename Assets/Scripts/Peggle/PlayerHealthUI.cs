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

    private float targetFill = 1f;
    private int targetHP;
    private int maxHPCached;
    private int lastShownHP = -1;
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
        if (healthFillImage != null)
        {
            // unscaled so the bar still animates while Time.timeScale = 0 (ability / turn notice)
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.unscaledDeltaTime * fillLerpSpeed);
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
        currentHP = Mathf.Max(0, currentHP);
        targetHP = currentHP;
        maxHPCached = maxHP;
        targetFill = maxHP > 0 ? Mathf.Clamp01((float)currentHP / maxHP) : 0f;

        isLow = maxHP > 0 && currentHP > 0 && (float)currentHP / maxHP <= lowHealthThreshold;
        if (!isLow) ResetTextPosition();
    }

    private void UpdateHealthText()
    {
        if (healthText == null || maxHPCached <= 0) return;

        float fill = healthFillImage.fillAmount;

        int shown = Mathf.Abs(fill - targetFill) < 0.002f
            ? targetHP                                   // snap to the exact final value
            : Mathf.RoundToInt(fill * maxHPCached);

        if (shown == lastShownHP) return;                // avoid rebuilding the text every frame
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