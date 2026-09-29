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

    private float targetFill = 1f;

    private void Awake()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f;
        }

        if (healthText != null)
        {
            healthText.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.deltaTime * fillLerpSpeed);
        }
    }

    public void UpdateHealth(int currentHP, int maxHP)
    {
        targetFill = Mathf.Clamp01((float)currentHP / maxHP);

        if (healthText != null)
        {
            healthText.text = $"{currentHP}/{maxHP}";
        }
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