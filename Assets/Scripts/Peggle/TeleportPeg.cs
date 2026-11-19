using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TeleportPeg : MonoBehaviour
{
    [Header("Pair")]
    [Tooltip("The portal this one sends balls to. You only need to set it on ONE of the two.")]
    [SerializeField] private TeleportPeg linkedPeg;
    [SerializeField] private Color pairColor = new Color(0.6f, 0.2f, 1f);

    [Header("Exit")]
    [Tooltip("Off = ball keeps its velocity. On = ball is shot out along the destination portal's up (green arrow).")]
    [SerializeField] private bool useExitDirection = false;
    [SerializeField] private float exitSpeedMultiplier = 1f;

    [Header("Swirl animation")]
    [Tooltip("Seconds the ball takes to spiral into the entry portal and shrink.")]
    [SerializeField] private float swirlInDuration = 0.3f;
    [Tooltip("Seconds the ball takes to grow back at the exit portal.")]
    [SerializeField] private float emergeDuration = 0.2f;
    [Tooltip("How many full turns the ball makes around the portal while spiraling in.")]
    [SerializeField] private float swirlTurns = 1.5f;
    [Tooltip("Degrees per second the ball itself spins during the animation.")]
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private bool swirlClockwise = true;

    [Header("Cooldown")]
    [Tooltip("Seconds both portals stay unusable after a teleport finishes.")]
    [SerializeField] private float cooldown = 0.75f;
    [Tooltip("Portal brightness while it is busy or cooling down (0 = black, 1 = no change).")]
    [SerializeField, Range(0f, 1f)] private float cooldownBrightness = 0.35f;

    [Header("Feedback")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField, Range(0f, 1f)] private float teleportVolume = 1f;
    [SerializeField] private float pulseScale = 1.3f;
    [SerializeField] private float pulseDuration = 0.15f;

    // Balls that just arrived here and must leave the trigger before they can teleport again
    private readonly HashSet<GameObject> ignoredBalls = new HashSet<GameObject>();

    private Renderer pegRenderer;
    private Material pegMat;
    private Vector3 baseScale;
    private Coroutine pulseRoutine;

    private bool busy;           // a ball is currently swirling through this portal
    private float cooldownEnd;   // Time.time when the cooldown finishes
    private float lastTint = -1f;

    private bool OnCooldown => busy || Time.time < cooldownEnd;

    private void Awake()
    {
        baseScale = transform.localScale;
        pegRenderer = GetComponent<Renderer>();
        if (pegRenderer != null) pegMat = pegRenderer.material;   // instance

        // Wire the partner back to us automatically
        if (linkedPeg != null && linkedPeg.linkedPeg == null)
            linkedPeg.linkedPeg = this;

        ApplyTint(1f);

        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
            Debug.LogWarning($"{name}: TeleportPeg needs its Collider2D set to 'Is Trigger'.", this);
    }

    private void Update()
    {
        // Dim while busy, then brighten back up over the cooldown
        float tint = 1f;
        if (busy)
        {
            tint = cooldownBrightness;
        }
        else if (Time.time < cooldownEnd && cooldown > 0f)
        {
            float remaining = (cooldownEnd - Time.time) / cooldown;
            tint = Mathf.Lerp(1f, cooldownBrightness, remaining);
        }

        if (!Mathf.Approximately(tint, lastTint)) ApplyTint(tint);
    }

    private void ApplyTint(float tint)
    {
        lastTint = tint;
        if (pegMat == null) return;

        Color c = pairColor * tint;
        c.a = pairColor.a;

        if (pegMat.HasProperty("_BaseColor")) pegMat.SetColor("_BaseColor", c);
        if (pegMat.HasProperty("_Color")) pegMat.SetColor("_Color", c);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (linkedPeg == null || OnCooldown || linkedPeg.OnCooldown) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic || !rb.simulated) return;

        ignoredBalls.RemoveWhere(b => b == null);
        if (ignoredBalls.Contains(rb.gameObject)) return;

        StartCoroutine(TeleportRoutine(rb, linkedPeg));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
            ignoredBalls.Remove(other.attachedRigidbody.gameObject);
    }

    private IEnumerator TeleportRoutine(Rigidbody2D rb, TeleportPeg dest)
    {
        GameObject ball = rb.gameObject;
        Transform bt = ball.transform;

        busy = true;
        dest.busy = true;

        // Read everything before freezing the body
        Vector2 velocity = rb.linearVelocity;   // rb.velocity on Unity 2022 or older
        float angularVelocity = rb.angularVelocity;
        Vector3 originalScale = bt.localScale;
        float z = bt.position.z;

        rb.simulated = false;   // no physics while the animation plays
        PlayFeedback();

        // ---- 1) Swirl into this portal and shrink ----
        Vector2 center = transform.position;
        Vector2 startOffset = (Vector2)bt.position - center;
        float startRadius = startOffset.magnitude;
        float startAngle = Mathf.Atan2(startOffset.y, startOffset.x);
        float dir = swirlClockwise ? -1f : 1f;

        float t = 0f;
        while (t < 1f)
        {
            if (ball == null) { busy = false; dest.busy = false; yield break; }

            t += Time.deltaTime / Mathf.Max(0.01f, swirlInDuration);
            float k = Mathf.Clamp01(t);
            float eased = k * k;   // accelerates toward the center

            float radius = Mathf.Lerp(startRadius, 0f, eased);
            float angle = startAngle + dir * k * swirlTurns * Mathf.PI * 2f;

            bt.position = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y + Mathf.Sin(angle) * radius,
                z);
            bt.localScale = Vector3.Lerp(originalScale, Vector3.zero, eased);
            bt.Rotate(0f, 0f, dir * spinSpeed * Time.deltaTime);

            yield return null;
        }

        // ---- 2) Jump to the partner and grow back ----
        Vector3 exitPos = dest.transform.position;
        bt.position = new Vector3(exitPos.x, exitPos.y, z);
        bt.localScale = Vector3.zero;
        dest.PlayFeedback();

        t = 0f;
        while (t < 1f)
        {
            if (ball == null) { busy = false; dest.busy = false; yield break; }

            t += Time.deltaTime / Mathf.Max(0.01f, emergeDuration);
            float k = Mathf.Clamp01(t);
            float eased = 1f - (1f - k) * (1f - k);   // decelerates as it reaches full size

            bt.localScale = Vector3.Lerp(Vector3.zero, originalScale, eased);
            bt.Rotate(0f, 0f, -dir * spinSpeed * Time.deltaTime);

            yield return null;
        }

        bt.localScale = originalScale;

        // ---- 3) Release the ball ----
        if (useExitDirection)
            velocity = (Vector2)dest.transform.up * velocity.magnitude;

        dest.ignoredBalls.Add(ball);   // it lands inside the exit trigger, so ignore it until it leaves

        rb.simulated = true;
        rb.linearVelocity = velocity * exitSpeedMultiplier;
        rb.angularVelocity = angularVelocity;

        // ---- 4) Cooldown on both portals ----
        busy = false;
        dest.busy = false;
        cooldownEnd = Time.time + cooldown;
        dest.cooldownEnd = Time.time + cooldown;
    }

    public void PlayFeedback()
    {
        if (teleportSound != null)
        {
            GameObject soundObj = new GameObject("TempTeleportAudio");
            soundObj.transform.position = transform.position;

            AudioSource src = soundObj.AddComponent<AudioSource>();
            src.clip = teleportSound;
            src.spatialBlend = 0f;           // 0 = fully 2D, no distance falloff
            src.volume = teleportVolume;
            src.Play();

            Destroy(soundObj, teleportSound.length);
        }

        // Only restart the pulse, never other coroutines (the teleport routine runs on this object too)
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        transform.localScale = baseScale;
        pulseRoutine = StartCoroutine(Pulse());
    }

    private IEnumerator Pulse()
    {
        float half = pulseDuration * 0.5f;
        Vector3 big = baseScale * pulseScale;
        float t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(baseScale, big, t / half);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(big, baseScale, t / half);
            yield return null;
        }

        transform.localScale = baseScale;
        pulseRoutine = null;
    }
}