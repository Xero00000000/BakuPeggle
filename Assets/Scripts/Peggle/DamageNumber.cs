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
    [SerializeField, Min(0f)] private float shadowStrength = 1f;

    [Header("Animación")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private float flyEndScale = 0.4f;

    [Header("Render")]
    [SerializeField] private int overlaySortingOrder = 500;   // higher than pegs, balls and board
    [SerializeField] private float bringToCameraDistance = 2f;

    private Vector3 baseScale;
    private Vector3 prefabScale;
    private Coroutine routine;
    private int baseSortingOrder;
    private bool sortingCaptured;

    private void Awake()
    {
        prefabScale = transform.localScale;
        baseScale = prefabScale;
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        // Draw over everything, ignoring depth. The shadow's queue is one lower, so it stays under the text.
        MakeOverlay(shadowTextMesh, 4000);
        MakeOverlay(textMesh, 4001);
    }

    private static void MakeOverlay(TMP_Text t, int queue)
    {
        if (t == null) return;

        Shader overlay = Shader.Find("TextMeshPro/Distance Field Overlay");
        if (overlay == null)
        {
            Debug.LogWarning("TMP overlay shader not found. Import TMP Essential Resources.");
            return;
        }

        Material m = new Material(t.fontSharedMaterial);   // copy keeps the font atlas and outline settings
        m.shader = overlay;                                // swap to the shader with ZTest Always
        m.renderQueue = queue;
        t.fontSharedMaterial = m;
    }

    public void SetText(string text)
    {
        if (textMesh != null) textMesh.text = text;
        if (shadowTextMesh != null) shadowTextMesh.text = text;
    }

    public void Show(string text, Color color, float sizeMultiplier = 1f)
    {
        baseScale = prefabScale * sizeMultiplier;
        transform.localScale = baseScale;

        if (textMesh != null)
        {
            textMesh.color = color;
            textMesh.sortingOrder = overlaySortingOrder + 1;   // main text on top
        }

        if (shadowTextMesh != null)
        {
            shadowTextMesh.color = shadowColor;

            if (textMesh != null)
            {
                shadowTextMesh.fontSize = textMesh.fontSize;
                shadowTextMesh.alignment = textMesh.alignment;
                shadowTextMesh.fontStyle = textMesh.fontStyle;
                shadowTextMesh.sortingLayerID = textMesh.sortingLayerID;
                shadowTextMesh.sortingOrder = overlaySortingOrder;   // shadow just below the text
            }
        }

        SetText(text);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PopRoutine());
        PlaceShadow();
    }

    public void BringToFront(Camera cam)
    {
        if (cam == null) return;
        transform.position -= cam.transform.forward * bringToCameraDistance;
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

    public void KeepInsideArea(Rect area, float padding = 0.1f)
    {
        if (textMesh == null) return;

        textMesh.ForceMeshUpdate();
        Bounds lb = textMesh.textBounds;

        // Use baseScale, not the current scale, because the pop animation starts tiny
        Vector2 ext = new Vector2(lb.extents.x, lb.extents.y) * baseScale.x * 1.15f;
        Vector2 center = (Vector2)transform.position + new Vector2(lb.center.x, lb.center.y) * baseScale.x;

        float minX = area.xMin + ext.x + padding, maxX = area.xMax - ext.x - padding;
        float minY = area.yMin + ext.y + padding, maxY = area.yMax - ext.y - padding;

        float cx = minX > maxX ? area.center.x : Mathf.Clamp(center.x, minX, maxX);
        float cy = minY > maxY ? area.center.y : Mathf.Clamp(center.y, minY, maxY);

        Vector3 p = transform.position;
        p.x += cx - center.x;
        p.y += cy - center.y;
        transform.position = p;
    }

    private void PlaceShadow()
    {
        if (shadowTextMesh == null) return;

        shadowTextMesh.transform.localRotation = Quaternion.identity;
        shadowTextMesh.transform.localPosition =
            new Vector3(shadowOffset.x, shadowOffset.y, 0f) * shadowStrength;
    }
}