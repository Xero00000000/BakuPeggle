using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("Referencias de Texto")]
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private TextMeshPro shadowTextMesh;

    [Header("Sombra")]
    [SerializeField] private Color shadowColor = Color.black;
    [SerializeField] private Vector3 shadowOffset = new Vector3(0.03f, -0.03f, 0.01f);

    [Header("Animación")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private float flyEndScale = 0.4f;

    private Vector3 baseScale;
    private Coroutine routine;

    private void Awake()
    {
        baseScale = transform.localScale;
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
    }

    public void SetText(string text)
    {
        if (textMesh != null) textMesh.text = text;
        if (shadowTextMesh != null) shadowTextMesh.text = text;
    }

    public void Show(string text, Color color, float sizeMultiplier = 1f)
    {
        transform.localScale = baseScale * sizeMultiplier;
        baseScale = transform.localScale;

        if (textMesh != null) textMesh.color = color;

        if (shadowTextMesh != null)
        {
            shadowTextMesh.color = shadowColor;
            shadowTextMesh.transform.localPosition = shadowOffset;
            if (textMesh != null)
            {
                shadowTextMesh.fontSize = textMesh.fontSize;
                shadowTextMesh.alignment = textMesh.alignment;
                shadowTextMesh.fontStyle = textMesh.fontStyle;
            }
        }

        SetText(text);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PopRoutine());
    }

    public void FlyTo(Vector3 target, Action onArrive)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlyRoutine(target, onArrive));
    }

    private IEnumerator PopRoutine()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / popDuration;
            float x = Mathf.Clamp01(t) - 1f;
            float easeOutBack = 1f + 2.70158f * x * x * x + 1.70158f * x * x;
            transform.localScale = baseScale * easeOutBack;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    private IEnumerator FlyRoutine(Vector3 target, Action onArrive)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / flyDuration;
            float e = Mathf.Clamp01(t);
            e = e * e * e; // starts slow, accelerates into the health bar
            transform.position = Vector3.Lerp(start, target, e);
            transform.localScale = Vector3.Lerp(baseScale, baseScale * flyEndScale, e);
            yield return null;
        }

        onArrive?.Invoke();
        Destroy(gameObject);
    }
}