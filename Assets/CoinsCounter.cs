using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Borra esta línea si usas Text clásico en vez de TextMeshPro

public class CoinsCounter : MonoBehaviour
{
    [Header("Texto de Monedas")]
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private Text legacyText;

    [Header("Valores")]
    [SerializeField] private int startCoins = 0;
    [SerializeField] private int targetCoins = 100;
    [SerializeField] private float duration = 1.5f;

    [Header("Ajustes")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";

    private Coroutine countCoroutine;

    void OnEnable()
    {
        StartCounting();
    }

    void OnDisable()
    {
        if (countCoroutine != null)
        {
            StopCoroutine(countCoroutine);
            countCoroutine = null;
        }
    }

    public void StartCounting()
    {
        if (countCoroutine != null)
        {
            StopCoroutine(countCoroutine);
        }

        countCoroutine = StartCoroutine(CountCoins(startCoins, targetCoins, duration));
    }

    // Útil si quieres cambiar el monto final desde otro script al activar el objeto
    public void SetTargetAndCount(int newTarget)
    {
        targetCoins = newTarget;
        if (gameObject.activeInHierarchy)
        {
            StartCounting();
        }
    }

    private IEnumerator CountCoins(int from, int to, float totalDuration)
    {
        UpdateText(from);

        if (totalDuration <= 0f)
        {
            UpdateText(to);
            yield break;
        }

        float elapsed = 0f;
        int lastValue = from;

        while (elapsed < totalDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / totalDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            int current = Mathf.RoundToInt(Mathf.Lerp(from, to, easedT));
            if (current != lastValue)
            {
                lastValue = current;
                UpdateText(current);
            }

            yield return null;
        }

        UpdateText(to);
        countCoroutine = null;
    }

    private void UpdateText(int amount)
    {
        string value = $"{prefix}{amount}{suffix}";
        if (tmpText != null) tmpText.text = value;
        if (legacyText != null) legacyText.text = value;
    }
}