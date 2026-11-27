using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreenSequencer : MonoBehaviour
{
    [Header("Sections (drag the objects from WinCanvas)")]
    [SerializeField] private GameObject[] titleObjects;    // Win n, Win n (1)
    [SerializeField] private GameObject[] coinObjects;     // Coin, CoinsNumber
    [SerializeField] private GameObject[] buttonObjects;   // Restart, Back

    [Header("Title: slides in from the left")]
    [SerializeField] private float delayBeforeTitle = 0.2f;
    [SerializeField] private float titleSlideDuration = 0.5f;
    [SerializeField] private float titleHold = 0.5f;

    [Header("Coins: slide up from below")]
    [SerializeField] private float coinSlideDuration = 0.5f;
    [SerializeField] private Animator coinAnimator;        // optional
    [SerializeField] private float coinsAnimationDuration = 1.5f;
    [SerializeField] private float coinAnimTimeout = 5f;

    [Header("Buttons")]
    [SerializeField] private float delayBeforeButtons = 0.3f;
    [SerializeField] private float fadeDuration = 0.3f;    // only if the object has a CanvasGroup

    private readonly Dictionary<RectTransform, Vector2> originalPos = new Dictionary<RectTransform, Vector2>();
    private RectTransform canvasRect;

    void OnEnable()
    {
        Canvas c = GetComponentInParent<Canvas>();
        canvasRect = c != null ? c.rootCanvas.GetComponent<RectTransform>() : null;

        // Remember the positions you placed in the editor (only the first time)
        Cache(titleObjects);
        Cache(coinObjects);

        // If the screen was interrupted last time, put everything back first
        RestorePositions(titleObjects);
        RestorePositions(coinObjects);

        SetActive(titleObjects, false);
        SetActive(coinObjects, false);
        SetActive(buttonObjects, false);

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return new WaitForSecondsRealtime(delayBeforeTitle);
        float w = canvasRect != null ? canvasRect.rect.width : Screen.width;
        yield return Slide(titleObjects, new Vector2(-w, 0f), titleSlideDuration);
        yield return new WaitForSecondsRealtime(titleHold);

        float h = canvasRect != null ? canvasRect.rect.height : Screen.height;
        yield return Slide(coinObjects, new Vector2(0f, -h), coinSlideDuration);
        yield return WaitForCoins();

        yield return new WaitForSecondsRealtime(delayBeforeButtons);
        yield return Reveal(buttonObjects);
    }

    // Slides each object from (its position + offset) to its original position
    IEnumerator Slide(GameObject[] objs, Vector2 offset, float duration)
    {
        var rects = new List<RectTransform>();

        foreach (var o in objs)
        {
            if (o == null) continue;
            var rt = o.GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.anchoredPosition = originalPos[rt] + offset;
            o.SetActive(true);
            rects.Add(rt);
        }

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            float ease = 1f - Mathf.Pow(1f - p, 3f);   // ease out

            foreach (var rt in rects)
                rt.anchoredPosition = Vector2.LerpUnclamped(originalPos[rt] + offset, originalPos[rt], ease);

            yield return null;
        }

        foreach (var rt in rects) rt.anchoredPosition = originalPos[rt];
    }

    IEnumerator WaitForCoins()
    {
        if (coinAnimator == null)
        {
            yield return new WaitForSecondsRealtime(coinsAnimationDuration);
            yield break;
        }

        yield return null;

        float timeout = coinAnimTimeout;
        while (timeout > 0f)
        {
            AnimatorStateInfo st = coinAnimator.GetCurrentAnimatorStateInfo(0);

            if (st.loop)
            {
                yield return new WaitForSecondsRealtime(coinsAnimationDuration);
                yield break;
            }
            if (st.normalizedTime >= 1f) yield break;

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator Reveal(GameObject[] objs)
    {
        var groups = new List<CanvasGroup>();

        foreach (var o in objs)
        {
            if (o == null) continue;
            o.SetActive(true);
            if (o.TryGetComponent(out CanvasGroup g))
            {
                g.alpha = 0f;
                groups.Add(g);
            }
        }

        if (groups.Count == 0 || fadeDuration <= 0f) yield break;

        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            foreach (var g in groups) g.alpha = t / fadeDuration;
            yield return null;
        }
        foreach (var g in groups) g.alpha = 1f;
    }

    void Cache(GameObject[] objs)
    {
        foreach (var o in objs)
        {
            if (o == null) continue;
            var rt = o.GetComponent<RectTransform>();
            if (rt != null && !originalPos.ContainsKey(rt))
                originalPos[rt] = rt.anchoredPosition;
        }
    }

    void RestorePositions(GameObject[] objs)
    {
        foreach (var o in objs)
        {
            if (o == null) continue;
            var rt = o.GetComponent<RectTransform>();
            if (rt != null && originalPos.TryGetValue(rt, out var p))
                rt.anchoredPosition = p;
        }
    }

    void SetActive(GameObject[] objs, bool active)
    {
        foreach (var o in objs)
            if (o != null) o.SetActive(active);
    }
}