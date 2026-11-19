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

    [Header("Wall bounce squash")]
    [SerializeField] private bool squashOnWall = true;
    [Tooltip("How much the ball flattens against the wall at full impact (0.3 = 30%).")]
    [SerializeField, Range(0f, 0.8f)] private float squashAmount = 0.3f;
    [Tooltip("How much it bulges sideways while squashed (0 = not at all, 1 = same as the squash).")]
    [SerializeField, Range(0f, 1f)] private float sidewaysBulge = 0.5f;
    [SerializeField] private float squashDuration = 0.3f;
    [Tooltip("How many times it wobbles back and forth before settling.")]
    [SerializeField, Min(0.5f)] private float wobbles = 1f;
    [Tooltip("Hits slower than this don't squash at all.")]
    [SerializeField] private float minImpactSpeed = 2f;
    [Tooltip("Hits at or above this speed squash the full amount.")]
    [SerializeField] private float fullImpactSpeed = 12f;
    [SerializeField] private float squashCooldown = 0.05f;
    [Tooltip("Keeps the physics collider the same size while the visual squashes.")]
    [SerializeField] private bool keepColliderSize = true;

    [Header("Shrink + vanish")]
    [SerializeField] private float shrinkDuration = 0.25f;
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private GameObject vanishVFXPrefab;
    [SerializeField] private float vanishVFXLifetime = 1.5f;

    public bool IsVanishing { get; private set; }

    private float lastWallVFXTime = -10f;
    private float lastSquashTime = -10f;

    private Coroutine squashRoutine;
    private Vector3 restScale;
    private float restRadius;
    private CircleCollider2D circle;

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
        if (IsVanishing) return;
        if (!IsWall(collision.collider)) return;

        Vector2 normal = Vector2.up;
        if (collision.contactCount > 0) normal = collision.GetContact(0).normal;

        if (squashOnWall) TrySquash(collision.relativeVelocity.magnitude, normal);

        if (wallHitVFXPrefab == null) return;
        if (Time.time - lastWallVFXTime < wallHitCooldown) return;
        lastWallVFXTime = Time.time;

        Vector3 pos = transform.position;
        if (collision.contactCount > 0)
        {
            ContactPoint2D contact = collision.GetContact(0);
            pos = new Vector3(contact.point.x, contact.point.y, transform.position.z);
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

    // ---------- Squash and stretch ----------
    private void TrySquash(float impactSpeed, Vector2 worldNormal)
    {
        if (impactSpeed < minImpactSpeed) return;
        if (Time.time - lastSquashTime < squashCooldown) return;
        lastSquashTime = Time.time;

        float strength = Mathf.Lerp(0.35f, 1f, Mathf.InverseLerp(minImpactSpeed, fullImpactSpeed, impactSpeed));

        // The ball spins, so work out the wall direction in the ball's own axes
        Vector3 local = transform.InverseTransformDirection(new Vector3(worldNormal.x, worldNormal.y, 0f));
        float nx2 = local.x * local.x;
        float ny2 = local.y * local.y;
        float sum = nx2 + ny2;
        if (sum < 0.0001f) { nx2 = 0.5f; ny2 = 0.5f; }
        else { nx2 /= sum; ny2 /= sum; }

        // If a squash is already running, keep its original rest size instead of the squashed one
        if (squashRoutine == null)
        {
            restScale = transform.localScale;
            circle = GetComponent<CircleCollider2D>();
            if (circle != null) restRadius = circle.radius;
        }
        else
        {
            StopCoroutine(squashRoutine);
        }

        squashRoutine = StartCoroutine(SquashRoutine(nx2, ny2, squashAmount * strength));
    }

    private IEnumerator SquashRoutine(float nx2, float ny2, float amplitude)
    {
        float t = 0f;
        float restMax = Mathf.Max(Mathf.Abs(restScale.x), Mathf.Abs(restScale.y));

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.05f, squashDuration);
            float k = Mathf.Clamp01(t);

            // Starts squashed, overshoots into a stretch, and settles
            float d = amplitude * Mathf.Exp(-4f * k) * Mathf.Cos(k * Mathf.PI * 2f * wobbles);

            float along = 1f - d;                    // against the wall
            float side = 1f + d * sidewaysBulge;     // perpendicular to it

            float sx = Mathf.Lerp(side, along, nx2);
            float sy = Mathf.Lerp(side, along, ny2);

            Vector3 s = new Vector3(restScale.x * sx, restScale.y * sy, restScale.z * side);
            transform.localScale = s;

            // A circle collider follows the biggest scale axis, so shrink the radius to cancel that out
            if (keepColliderSize && circle != null)
            {
                float curMax = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
                if (curMax > 0.0001f) circle.radius = restRadius * restMax / curMax;
            }

            yield return null;
        }

        ResetSquash();
    }

    // Public so other scripts (teleport, vanish) can cancel it before they touch the ball's scale
    public void ResetSquash()
    {
        if (squashRoutine == null) return;

        StopCoroutine(squashRoutine);
        squashRoutine = null;

        transform.localScale = restScale;
        if (circle != null) circle.radius = restRadius;
    }

    // ---------- Shrink + vanish ----------
    public void ShrinkAndVanish()
    {
        if (IsVanishing) return;
        ResetSquash();
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