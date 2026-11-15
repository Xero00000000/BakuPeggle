using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Referencias de Texto")]
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private TextMeshPro shadowTextMesh;

    [Header("Configuración de la Sombra")]
    [SerializeField] private Color shadowColor = Color.black;
    [SerializeField] private Vector3 shadowOffset = new Vector3(0.03f, -0.03f, 0.01f);

    [Header("Configuración de Animación")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Color coincidente (boost)")]
    [SerializeField] private float boostScale = 1.5f;
    [SerializeField] private float punchScale = 1.25f;     // extra pop on each boosted hit
    [SerializeField] private float punchDuration = 0.15f;
    [SerializeField, Range(0f, 1f)] private float glowPower = 0.7f;
    [SerializeField, Range(0f, 1f)] private float glowWhiteMix = 0.35f;

    [Header("Evitar la barra de marbeasts")]
    [Tooltip("Left edge of the screen (0 to 1) that the LEFT EDGE of the text can't go past.")]
    [SerializeField, Range(0f, 0.6f)] private float minViewportX = 0.2f;
    [Tooltip("Right edge of the screen (0 to 1) that the RIGHT EDGE of the text can't go past.")]
    [SerializeField, Range(0.5f, 1f)] private float maxViewportX = 0.97f;

    private float timeLeft;
    private Color mainColor = Color.white;
    private int baseSortingOrder;
    private bool sortingCaptured;

    private Vector3 baseScale;
    private bool scaleCaptured;
    private bool boosted;
    private float punchLeft;
    private Material textMat;

    private Vector3 basePos;   // where the text wants to be, before the clamp shifts it

    private void CaptureBase()
    {
        if (!scaleCaptured)
        {
            baseScale = transform.localScale;
            scaleCaptured = true;
        }
    }

    // Uses the text's real bounds (scale included) so a bigger text is pushed further in
    private void ClampToPlayArea()
    {
        Camera cam = Camera.main;
        if (cam == null || textMesh == null) return;

        Bounds b = textMesh.renderer.bounds;   // world-space bounds of the text mesh at its current scale

        float depth = cam.WorldToViewportPoint(b.center).z;
        float leftLimit = cam.ViewportToWorldPoint(new Vector3(minViewportX, 0.5f, depth)).x;
        float rightLimit = cam.ViewportToWorldPoint(new Vector3(maxViewportX, 0.5f, depth)).x;

        float shift = 0f;
        if (b.min.x < leftLimit) shift = leftLimit - b.min.x;
        else if (b.max.x > rightLimit) shift = rightLimit - b.max.x;

        if (shift != 0f)
            transform.position += new Vector3(shift, 0f, 0f);
    }

    public void Setup(string text, Color color, bool boost = false)
    {
        CaptureBase();
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        mainColor = color;
        boosted = boost;

        // PeggleUIManager sets the position before calling Setup
        basePos = transform.position;

        if (!sortingCaptured && textMesh != null)
        {
            baseSortingOrder = textMesh.sortingOrder;
            sortingCaptured = true;
        }

        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textMesh.sortingOrder = baseSortingOrder + 1;
            ApplyGlow(boost, color);
            textMesh.ForceMeshUpdate();   // so the bounds match the new text right away
        }

        if (shadowTextMesh != null)
        {
            shadowTextMesh.text = text;
            shadowTextMesh.color = shadowColor;
            shadowTextMesh.transform.localPosition = shadowOffset;
            if (textMesh != null)
            {
                shadowTextMesh.fontSize = textMesh.fontSize;
                shadowTextMesh.alignment = textMesh.alignment;
                shadowTextMesh.fontStyle = textMesh.fontStyle;
                shadowTextMesh.sortingLayerID = textMesh.sortingLayerID;
                shadowTextMesh.sortingOrder = baseSortingOrder;
            }
        }

        transform.localScale = baseScale * (boost ? boostScale : 1f);
        punchLeft = boost ? punchDuration : 0f;

        timeLeft = fadeDuration;
    }

    private void ApplyGlow(bool on, Color color)
    {
        // fontMaterial returns a per-object instance, so other texts are not affected
        if (textMat == null) textMat = textMesh.fontMaterial;
        if (textMat == null) return;

        if (on)
        {
            textMat.EnableKeyword(ShaderUtilities.Keyword_Glow);
            textMat.SetColor(ShaderUtilities.ID_GlowColor, Color.Lerp(color, Color.white, glowWhiteMix));
            textMat.SetFloat(ShaderUtilities.ID_GlowOffset, 0f);
            textMat.SetFloat(ShaderUtilities.ID_GlowInner, 0.1f);
            textMat.SetFloat(ShaderUtilities.ID_GlowOuter, 0.5f);
            textMat.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);
        }
        else
        {
            textMat.DisableKeyword(ShaderUtilities.Keyword_Glow);
            textMat.SetFloat(ShaderUtilities.ID_GlowPower, 0f);
        }
    }

    private void Update()
    {
        basePos += Vector3.up * moveSpeed * Time.deltaTime;
        transform.position = basePos;   // clamp runs after scaling, in LateUpdate

        timeLeft -= Time.deltaTime;
        float a = Mathf.Clamp01(timeLeft / fadeDuration);

        // Pop that settles back to the boosted size
        if (boosted)
        {
            float k = 1f;
            if (punchLeft > 0f)
            {
                punchLeft -= Time.deltaTime;
                float p = Mathf.Clamp01(punchLeft / punchDuration);
                k = 1f + (punchScale - 1f) * p;
            }
            transform.localScale = baseScale * boostScale * k;

            if (textMat != null) textMat.SetFloat(ShaderUtilities.ID_GlowPower, glowPower * a);
        }

        if (textMesh != null)
        {
            Color c = mainColor; c.a = mainColor.a * a;
            textMesh.color = c;
        }
        if (shadowTextMesh != null)
        {
            Color sc = shadowColor; sc.a = shadowColor.a * a;
            shadowTextMesh.color = sc;
        }

        if (timeLeft <= 0f) Destroy(gameObject);
    }

    private void LateUpdate()
    {
        // After Update has set position and scale for this frame
        ClampToPlayArea();
    }
}