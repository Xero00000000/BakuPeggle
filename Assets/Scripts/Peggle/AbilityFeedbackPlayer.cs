using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Everything runs on unscaled time because PeggleUIManager freezes the game (timeScale = 0) during abilities.
public class AbilityFeedbackPlayer : MonoBehaviour
{
    [Header("Per-ability data (0 Damage, 1 Heal, 2 Clone, 3 Refresh)")]
    [SerializeField] private AbilityFeedbackData[] abilities;

    [Header("References")]
    [SerializeField] private DamageEffectManager damageEffects;
    [Tooltip("Full-screen Image (raycast off). Used for the color flash of Pulse Player abilities. Must be a different GameObject from this script.")]
    [SerializeField] private Image overlay;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private Camera cam;

    [Header("Cut-in recolor")]
    [Tooltip("Every Image of the cut-in art. Each one must use a material with the UI/HueShift shader.")]
    [SerializeField] private List<Image> cutInImages = new List<Image>();
    [Tooltip("Hue of the ORIGINAL art, 0-1. Red = 0, green = 0.33, blue = 0.66. Your art is red, so 0.")]
    [SerializeField, Range(0f, 1f)] private float artBaseHue = 0f;

    [Header("UI reactions")]
    [SerializeField] private List<RectTransform> enemyUI = new List<RectTransform>();

    private static readonly int HueId = Shader.PropertyToID("_Hue");

    private ChromaticAberration chroma;
    private readonly Dictionary<RectTransform, Vector2> restPos = new Dictionary<RectTransform, Vector2>();
    private readonly List<Material> cutInMats = new List<Material>();

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (globalVolume == null) globalVolume = FindFirstObjectByType<Volume>();
        if (globalVolume != null && globalVolume.profile != null)
            globalVolume.profile.TryGet(out chroma);

        if (overlay != null)
        {
            overlay.raycastTarget = false;
            overlay.gameObject.SetActive(false);
        }

        // Own copy of the material per image so the project asset is never modified
        foreach (var img in cutInImages)
        {
            if (img == null) continue;
            Material m = new Material(img.material);
            img.material = m;
            cutInMats.Add(m);
        }
    }

    private void OnDestroy()
    {
        foreach (var m in cutInMats)
            if (m != null) Destroy(m);
    }

    public AbilityFeedbackData Get(int index)
    {
        if (abilities == null || index < 0 || index >= abilities.Length) return null;
        return abilities[index];
    }

    // ---------------------------------------------------------------- CUT-IN

    public IEnumerator PlayIntro(AbilityFeedbackData fx, List<RectTransform> cutIn)
    {
        PlaySound(fx.introSound);
        SetCutInHue(fx.color);

        Vector2 offset = SlideOffset(fx.slideFrom);

        foreach (var rt in cutIn)
        {
            if (rt == null) continue;

            // Rest position: x = 0 like before, y as authored in the editor
            if (!restPos.ContainsKey(rt)) restPos[rt] = new Vector2(0f, rt.anchoredPosition.y);

            rt.gameObject.SetActive(true);
            CanvasGroup g = rt.GetComponent<CanvasGroup>();
            if (g == null) g = rt.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 1f;
            rt.anchoredPosition = restPos[rt] + offset;
        }

        float dur = Mathf.Max(0.01f, fx.slideInDuration);
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float eased = 1f - Mathf.Pow(1f - k, 3f);

            foreach (var rt in cutIn)
            {
                if (rt == null) continue;
                rt.anchoredPosition = Vector2.LerpUnclamped(restPos[rt] + offset, restPos[rt], eased);
            }

            yield return null;
        }

        foreach (var rt in cutIn)
            if (rt != null) rt.anchoredPosition = restPos[rt];
    }

    // ---------------------------------------------------------------- IMPACT

    // Runs after the cut-in has faded out.
    // onImpact fires at the exact moment of impact (use it to update the health bars).
    public IEnumerator PlayImpact(AbilityFeedbackData fx, System.Action onImpact = null)
    {
        if (fx.anticipationPause > 0f)
            yield return new WaitForSecondsRealtime(fx.anticipationPause);

        PlaySound(fx.impactSound);
        if (onImpact != null) onImpact.Invoke();

        bool wantsColorFlash = fx.uiReaction == UIReaction.PulsePlayer;
        bool wantsLines = fx.uiReaction == UIReaction.ShakeEnemy && fx.useLines && fx.lineIntensity > 0f;

        float longest = 0f;

        // Screen color flash: only for Pulse Player
        if (wantsColorFlash && overlay != null)
        {
            StartCoroutine(ColorPulse(fx));
            longest = Mathf.Max(longest, fx.flashDuration);
        }

        // Speed lines: only for Shake Enemy. Camera shake follows the Shake value (0 = none).
        if (damageEffects != null && (fx.shake > 0f || wantsLines))
        {
            damageEffects.TriggerCustom(fx.color, wantsLines ? fx.lineIntensity : 0f, fx.effectDuration, fx.shake);
            longest = Mathf.Max(longest, fx.effectDuration);
        }

        if (fx.aberrationPeak > 0f && chroma != null)
        {
            StartCoroutine(AberrationPulse(fx.aberrationPeak, fx.aberrationDuration));
            longest = Mathf.Max(longest, fx.aberrationDuration);
        }

        if (fx.zoomPunch > 0f && cam != null)
        {
            StartCoroutine(ZoomPunch(fx.zoomPunch, fx.zoomDuration));
            longest = Mathf.Max(longest, fx.zoomDuration);
        }

        if (fx.uiReaction == UIReaction.ShakeEnemy)
        {
            StartCoroutine(ShakeUI(enemyUI, fx.uiShakePixels, fx.uiDuration));
            longest = Mathf.Max(longest, fx.uiDuration);
        }

        // Pegs spawned by Refresh pop in now, staggered
        FeedbackBalls.ReleaseSpawnPops(fx.pegPopStagger);

        yield return new WaitForSecondsRealtime(longest);
    }

    // ---------------------------------------------------------------- EFFECTS

    // Swells in and fades out in the ability's color
    private IEnumerator ColorPulse(AbilityFeedbackData fx)
    {
        overlay.gameObject.SetActive(true);

        float dur = Mathf.Max(0.01f, fx.flashDuration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI);
            SetOverlay(fx.color, fx.flashAlpha * k);
            yield return null;
        }

        SetOverlay(fx.color, 0f);
        overlay.gameObject.SetActive(false);
    }

    private IEnumerator AberrationPulse(float peak, float duration)
    {
        chroma.intensity.overrideState = true;
        float start = chroma.intensity.value;
        float top = Mathf.Max(peak, start);
        float dur = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            chroma.intensity.value = Mathf.Lerp(top, start, t / dur);
            yield return null;
        }

        chroma.intensity.value = start;
    }

    private IEnumerator ZoomPunch(float amount, float duration)
    {
        bool ortho = cam.orthographic;
        float baseValue = ortho ? cam.orthographicSize : cam.fieldOfView;
        float dur = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI);   // in and back out
            float v = baseValue * (1f - amount * k);
            if (ortho) cam.orthographicSize = v; else cam.fieldOfView = v;
            yield return null;
        }

        if (ortho) cam.orthographicSize = baseValue; else cam.fieldOfView = baseValue;
    }

    private IEnumerator ShakeUI(List<RectTransform> list, float pixels, float duration)
    {
        if (list == null || list.Count == 0) yield break;

        var origin = new List<Vector2>();
        foreach (var rt in list) origin.Add(rt != null ? rt.anchoredPosition : Vector2.zero);

        float dur = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float damp = 1f - Mathf.Clamp01(t / dur);

            for (int i = 0; i < list.Count; i++)
                if (list[i] != null)
                    list[i].anchoredPosition = origin[i] + Random.insideUnitCircle * pixels * damp;

            yield return null;
        }

        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) list[i].anchoredPosition = origin[i];
    }

    // ---------------------------------------------------------------- HELPERS

    private void SetOverlay(Color c, float alpha)
    {
        if (overlay == null) return;
        c.a = alpha;
        overlay.color = c;
    }

    // Rotates the hue of the cut-in art from its original hue to the ability's color
    private void SetCutInHue(Color target)
    {
        Color.RGBToHSV(target, out float h, out float s, out float v);
        float shift = Mathf.Repeat(h - artBaseHue, 1f);

        foreach (var m in cutInMats)
            if (m != null && m.HasProperty(HueId))
                m.SetFloat(HueId, shift);
    }

    private static Vector2 SlideOffset(SlideDirection d)
    {
        switch (d)
        {
            case SlideDirection.FromLeft: return new Vector2(-Screen.width, 0f);
            case SlideDirection.FromRight: return new Vector2(Screen.width, 0f);
            case SlideDirection.FromTop: return new Vector2(0f, Screen.height);
            default: return new Vector2(0f, -Screen.height);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        GameObject obj = new GameObject("TempAbilityAudio");
        AudioSource src = obj.AddComponent<AudioSource>();
        src.clip = clip;
        src.ignoreListenerPause = true;
        src.Play();
        Destroy(obj, clip.length);
    }
}