using UnityEngine;
using UnityEngine.Rendering;

public class BallComboGlow : MonoBehaviour
{
    [Header("Side")]
    [Tooltip("Default only. The spawners override it with SetSide().")]
    [SerializeField] private bool isPlayerBall = true;

    [Header("Outline")]
    [Tooltip("Drag BallOutlineMat here (URP Unlit, Render Face = Back). A material asset keeps the shader in builds.")]
    [SerializeField] private Material outlineMaterialAsset;
    [Tooltip("Set this per ball prefab to match that ball's color. Mixing in some white hides chromatic aberration fringing better.")]
    [ColorUsage(true, true)]
    [SerializeField] private Color glowColor = Color.red;
    [Tooltip("Combo hits needed to reach the full outline.")]
    [SerializeField] private int hitsToMax = 8;
    [Tooltip("Outline thickness at max combo (0.16 = 16% bigger than the ball).")]
    [SerializeField] private float maxOutlineSize = 0.16f;
    [SerializeField] private float fadeSpeed = 8f;

    [Header("Pulse at max combo")]
    [SerializeField] private float pulseAmount = 0.02f;
    [SerializeField] private float pulseSpeed = 8f;

    // Mesh outline
    private MeshRenderer outlineMesh;
    private Material outlineMat;
    private Vector3 meshCenter;

    // Sprite outline (fallback)
    private SpriteRenderer ballSprite;
    private SpriteRenderer outlineSprite;

    private Transform outline;
    private float current;   // smoothed 0..1

    public void SetSide(bool player)
    {
        isPlayerBall = player;
    }

    private void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        ballSprite = GetComponent<SpriteRenderer>();

        if (mf != null && mf.sharedMesh != null)
            BuildMeshOutline(mf);
        else if (ballSprite != null)
            BuildSpriteOutline();
        else
        {
            Debug.LogWarning($"{name}: BallComboGlow needs a MeshFilter or a SpriteRenderer on the ball.", this);
            enabled = false;
        }
    }

    private void BuildMeshOutline(MeshFilter mf)
    {
        if (outlineMaterialAsset == null)
        {
            Debug.LogError($"{name}: assign Outline Material Asset on BallComboGlow (Shader.Find is unreliable in builds).", this);
            enabled = false;
            return;
        }

        GameObject go = new GameObject("ComboOutline");
        outline = go.transform;
        outline.SetParent(transform, false);

        meshCenter = mf.sharedMesh.bounds.center;

        go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
        outlineMesh = go.AddComponent<MeshRenderer>();

        outlineMat = new Material(outlineMaterialAsset);   // per-ball instance
        outlineMat.SetFloat("_Cull", (float)CullMode.Front);   // safety: back faces only

        outlineMesh.sharedMaterial = outlineMat;
        outlineMesh.shadowCastingMode = ShadowCastingMode.Off;
        outlineMesh.receiveShadows = false;
        outlineMesh.enabled = false;
    }

    private void BuildSpriteOutline()
    {
        GameObject go = new GameObject("ComboOutline");
        outline = go.transform;
        outline.SetParent(transform, false);

        outlineSprite = go.AddComponent<SpriteRenderer>();
        outlineSprite.sprite = ballSprite.sprite;
        outlineSprite.sortingLayerID = ballSprite.sortingLayerID;
        outlineSprite.sortingOrder = ballSprite.sortingOrder - 1;
        outlineSprite.enabled = false;
    }

    private void Update()
    {
        int streak = isPlayerBall ? FeedbackBalls.PlayerStreak : FeedbackBalls.EnemyStreak;
        float target = Mathf.Clamp01(streak / (float)Mathf.Max(1, hitsToMax));

        current = Mathf.Lerp(current, target, Time.unscaledDeltaTime * fadeSpeed);

        bool visible = current >= 0.01f;
        if (outlineMesh != null) outlineMesh.enabled = visible;
        if (outlineSprite != null) outlineSprite.enabled = visible;
        if (!visible) return;

        float size = current * maxOutlineSize;
        if (target >= 1f)
            size += (Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f) * pulseAmount;

        float s = 1f + size;
        outline.localScale = Vector3.one * s;

        if (outlineMesh != null)
            outline.localPosition = meshCenter * (1f - s);

        if (outlineMat != null)
        {
            if (outlineMat.HasProperty("_BaseColor")) outlineMat.SetColor("_BaseColor", glowColor);
            if (outlineMat.HasProperty("_Color")) outlineMat.SetColor("_Color", glowColor);
        }

        if (outlineSprite != null)
        {
            outlineSprite.sprite = ballSprite.sprite;
            Color c = glowColor;
            c.a = current;
            outlineSprite.color = c;
        }
    }

    private void OnDestroy()
    {
        if (outlineMat != null) Destroy(outlineMat);
    }
}