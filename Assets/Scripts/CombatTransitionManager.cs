using System;
using System.Collections;
using UnityEngine;

public class CombatTransitionManager : MonoBehaviour
{
    public static CombatTransitionManager Instance;

    [Header("Referencias UI")]
    public CanvasGroup flashOverlay;

    [Header("Tiempos de Transición")]
    public float flashInDuration = 0.2f;
    public float holdDuration = 0.15f;
    public float flashOutDuration = 0.4f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (flashOverlay != null)
        {
            flashOverlay.alpha = 0f;
        }
    }

    public void TriggerTransition(Action onPeakAction = null, Action onComplete = null)
    {
        StartCoroutine(FlashRoutine(onPeakAction, onComplete));
    }

    private IEnumerator FlashRoutine(Action onPeakAction, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            flashOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / flashInDuration);
            yield return null;
        }
        flashOverlay.alpha = 1f;

        onPeakAction?.Invoke();

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < flashOutDuration)
        {
            elapsed += Time.deltaTime;
            flashOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / flashOutDuration);
            yield return null;
        }
        flashOverlay.alpha = 0f;

        onComplete?.Invoke();
    }
}