using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class MarbeastSphereVanish : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float riseHeight = 1.5f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float spinSpeed = 360f;
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField]
    private AnimationCurve shrinkCurve = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(0.5f, 0.9f), new Keyframe(1f, 0f));

    [Header("VFX Graph (optional)")]
    [SerializeField] private VisualEffect vfx;           // can be a child or a sibling, both work
    [SerializeField] private float vfxDelay = 0f;        // extra wait after the sphere vanishes
    [SerializeField] private float vfxLingerTime = 1.5f; // how long the VFX stays enabled
    [SerializeField] private bool debugLog = true;

    private Vector3 startPos;
    private Vector3 startScale;
    private Quaternion startRot;
    private bool cached;
    private bool playing;
    private Coroutine routine;

    // VFX original parent info, so Restore() can put it back
    private Transform vfxOriginalParent;
    private Vector3 vfxOriginalLocalPos;
    private Quaternion vfxOriginalLocalRot;
    private Vector3 vfxOriginalLocalScale;

    void Cache()
    {
        if (cached) return;
        startPos = transform.localPosition;
        startScale = transform.localScale;
        startRot = transform.localRotation;
        cached = true;

        if (vfx != null)
        {
            vfxOriginalParent = vfx.transform.parent;
            vfxOriginalLocalPos = vfx.transform.localPosition;
            vfxOriginalLocalRot = vfx.transform.localRotation;
            vfxOriginalLocalScale = vfx.transform.localScale;

            vfx.gameObject.SetActive(false);
        }
    }

    public void Play()
    {
        Cache();
        if (playing) return;
        routine = StartCoroutine(Animate());
    }

    public void SetVanished()
    {
        Cache();
        if (playing) return;
        transform.localScale = Vector3.zero;
    }

    public void Restore()
    {
        Cache();
        if (routine != null) StopCoroutine(routine);
        playing = false;

        if (vfx != null)
        {
            vfx.gameObject.SetActive(false);
            ReattachVfx();
        }

        transform.localPosition = startPos;
        transform.localScale = startScale;
        transform.localRotation = startRot;
    }

    IEnumerator Animate()
    {
        playing = true;

        // 0) Detach the VFX from the sphere so the sphere's scale can't affect it
        if (vfx != null && vfx.transform.IsChildOf(transform))
        {
            vfx.transform.SetParent(null, true);
            vfx.transform.localScale = Vector3.one;
            vfx.transform.rotation = Quaternion.identity;
        }
        else if (vfx != null)
        {
            vfx.transform.localScale = Vector3.one;
        }

        // 1) Rise + shrink (no particles yet)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float k = Mathf.Clamp01(t);

            transform.localPosition = startPos + Vector3.up * (riseHeight * riseCurve.Evaluate(k));
            transform.localScale = startScale * shrinkCurve.Evaluate(k);
            if (spinSpeed != 0f) transform.Rotate(Vector3.up, spinSpeed * Time.unscaledDeltaTime, Space.World);

            yield return null;
        }

        // 2) Sphere is gone
        Vector3 vanishPos = transform.position;
        transform.localScale = Vector3.zero;

        // 3) Particles appear where it vanished
        if (vfx != null)
        {
            if (vfxDelay > 0f) yield return new WaitForSecondsRealtime(vfxDelay);

            vfx.transform.position = vanishPos;
            vfx.gameObject.SetActive(true);
            vfx.Reinit();
            vfx.Play();

            if (debugLog)
            {
                yield return null;
                Debug.Log($"[MarbeastSphereVanish] VFX alive particles: {vfx.aliveParticleCount} | pos: {vfx.transform.position} | lossyScale: {vfx.transform.lossyScale}");
            }

            yield return new WaitForSecondsRealtime(vfxLingerTime);
            vfx.gameObject.SetActive(false);
            ReattachVfx();
        }

        playing = false;
        routine = null;
    }

    // Put the VFX back under its original parent (so Reset Store / next play start clean)
    void ReattachVfx()
    {
        if (vfx == null || vfxOriginalParent == null) return;

        vfx.transform.SetParent(vfxOriginalParent, false);
        vfx.transform.localPosition = vfxOriginalLocalPos;
        vfx.transform.localRotation = vfxOriginalLocalRot;
        vfx.transform.localScale = vfxOriginalLocalScale;
    }
}