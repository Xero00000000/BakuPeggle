using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class BallEffects : MonoBehaviour
{
    public enum ColorSource { Manual, FromBallRenderer }

    [Header("VFX color")]
    [Tooltip("Manual = use the color below. FromBallRenderer = read it from the ball's sprite/material.")]
    [SerializeField] private ColorSource colorSource = ColorSource.Manual;
    [SerializeField] private Color manualColor = Color.white;
    [Tooltip("Multiplies the color's brightness (useful if the graph's color is HDR).")]
    [SerializeField, Min(0f)] private float colorIntensity = 1f;
    [Tooltip("Exposed Color properties in the VFX Graph that receive the ball color. Add 'Explosion color' / 'Fire color' if the graph has them.")]
    [SerializeField] private string[] vfxColorProperties = { "Color" };
    [SerializeField] private bool tintWallHit = false;

    [Header("Wall hit VFX")]
    [Tooltip("Any prefab that contains a Visual Effect (root or child).")]
    [SerializeField] private GameObject wallHitVFXPrefab;
    [Tooltip("Layers that count as walls. If left on Nothing, anything that isn't a peg or another ball counts as a wall.")]
    [SerializeField] private LayerMask wallLayers;
    [SerializeField] private bool alignToNormal = true;
    [SerializeField] private float wallHitCooldown = 0.08f;
    [SerializeField] private float wallVFXLifetime = 1f;

    [Header("Shrink + vanish")]
    [SerializeField] private float shrinkDuration = 0.25f;
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private GameObject vanishVFXPrefab;
    [SerializeField] private float vanishVFXLifetime = 1.5f;

    public bool IsVanishing { get; private set; }

    private float lastWallVFXTime = -10f;

    // ---------- Color ----------
    private Color ResolveColor()
    {
        Color c = manualColor;

        if (colorSource == ColorSource.FromBallRenderer)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                c = sr.color;
            }
            else
            {
                Renderer r = GetComponentInChildren<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    Material m = r.sharedMaterial;
                    if (m.HasProperty("_BaseColor")) c = m.GetColor("_BaseColor");
                    else if (m.HasProperty("_Color")) c = m.GetColor("_Color");
                }
            }
        }

        c.r *= colorIntensity;
        c.g *= colorIntensity;
        c.b *= colorIntensity;
        return c;
    }

    // ---------- Wall hit ----------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsVanishing || wallHitVFXPrefab == null) return;
        if (!IsWall(collision.collider)) return;
        if (Time.time - lastWallVFXTime < wallHitCooldown) return;
        lastWallVFXTime = Time.time;

        Vector3 pos = transform.position;
        Vector3 normal = Vector3.up;

        if (collision.contactCount > 0)
        {
            ContactPoint2D contact = collision.GetContact(0);
            pos = new Vector3(contact.point.x, contact.point.y, transform.position.z);
            normal = contact.normal;
        }

        Quaternion rot = alignToNormal ? Quaternion.FromToRotation(Vector3.up, normal) : Quaternion.identity;
        Spawn(wallHitVFXPrefab, pos, rot, wallVFXLifetime, tintWallHit);
    }

    private bool IsWall(Collider2D other)
    {
        if (wallLayers.value != 0)
            return (wallLayers.value & (1 << other.gameObject.layer)) != 0;

        return other.GetComponentInParent<FeedbackBalls>() == null
            && other.GetComponentInParent<BallEffects>() == null;
    }

    // ---------- Shrink + vanish ----------
    public void ShrinkAndVanish()
    {
        if (IsVanishing) return;
        IsVanishing = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, shrinkDuration);
            transform.localScale = startScale * shrinkCurve.Evaluate(Mathf.Clamp01(t));
            yield return null;
        }

        transform.localScale = Vector3.zero;

        if (vanishVFXPrefab != null)
            Spawn(vanishVFXPrefab, transform.position, Quaternion.identity, vanishVFXLifetime, true);

        Destroy(gameObject);
    }

    // Works with a prefab whose VisualEffect is on the root or on any child
    private void Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime, bool tint)
    {
        GameObject go = Instantiate(prefab, position, rotation);   // scene root, so the ball's scale can't affect it
        go.SetActive(true);

        Color color = ResolveColor();

        foreach (VisualEffect vfx in go.GetComponentsInChildren<VisualEffect>(true))
        {
            vfx.gameObject.SetActive(true);

            if (tint && vfxColorProperties != null)
            {
                foreach (string prop in vfxColorProperties)
                {
                    if (string.IsNullOrEmpty(prop)) continue;
                    int id = Shader.PropertyToID(prop);
                    if (vfx.HasVector4(id)) vfx.SetVector4(id, color);
                }
            }

            vfx.Reinit();
            vfx.Play();
        }

        Destroy(go, lifetime);
    }
}