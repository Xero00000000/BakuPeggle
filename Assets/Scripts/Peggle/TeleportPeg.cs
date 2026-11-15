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
    [Tooltip("Off = ball keeps its velocity. On = ball is shot out along this portal's up (green arrow).")]
    [SerializeField] private bool useExitDirection = false;
    [SerializeField] private float exitSpeedMultiplier = 1f;

    [Header("Feedback")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private float pulseScale = 1.3f;
    [SerializeField] private float pulseDuration = 0.15f;

    // Balls that just arrived here and must leave the trigger before they can teleport again
    private readonly HashSet<GameObject> ignoredBalls = new HashSet<GameObject>();

    private Renderer pegRenderer;
    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
        pegRenderer = GetComponent<Renderer>();

        // Wire the partner back to us automatically
        if (linkedPeg != null && linkedPeg.linkedPeg == null)
            linkedPeg.linkedPeg = this;

        ApplyColor();

        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
            Debug.LogWarning($"{name}: TeleportPeg needs its Collider2D set to 'Is Trigger'.", this);
    }

    private void ApplyColor()
    {
        if (pegRenderer == null) return;

        Material m = pegRenderer.material;   // instance
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", pairColor);
        if (m.HasProperty("_Color")) m.SetColor("_Color", pairColor);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (linkedPeg == null) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic) return;

        GameObject ball = rb.gameObject;

        ignoredBalls.RemoveWhere(b => b == null);
        if (ignoredBalls.Contains(ball)) return;

        // Tell the destination to ignore this ball until it leaves that portal
        linkedPeg.ignoredBalls.Add(ball);

        Vector2 velocity = rb.linearVelocity;   // rb.velocity on Unity 2022 or older

        if (useExitDirection)
            velocity = (Vector2)linkedPeg.transform.up * velocity.magnitude;

        rb.position = linkedPeg.transform.position;
        rb.linearVelocity = velocity * exitSpeedMultiplier;

        PlayFeedback();
        linkedPeg.PlayFeedback();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
            ignoredBalls.Remove(other.attachedRigidbody.gameObject);
    }

    public void PlayFeedback()
    {
        if (teleportSound != null)
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);

        StopAllCoroutines();
        StartCoroutine(Pulse());
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
    }
}