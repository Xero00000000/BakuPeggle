using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CellphonePauseSystem : MonoBehaviour
{
    [Header("Referencias de UI")]
    public RectTransform phoneFullRect;     
    public RectTransform phoneMiniRect;     
    public GameObject fullPhoneContainer;     

    [Header("Configuración de Animación")]
    public float animationDuration = 0.28f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Escenas de Navegación")]
    public string streetSceneName = "StreetScene";
    public string settingsSceneName = "SettingsScene";

    private bool isPaused = false;
    private Coroutine transitionCoroutine;

    private Vector2 initialCenterAnchoredPos;
    private Vector3 fullScale = Vector3.one;

    void Awake()
    {
        if (phoneFullRect != null)
        {
            initialCenterAnchoredPos = phoneFullRect.anchoredPosition;
        }
    }

    void Start()
    {
        if (fullPhoneContainer != null)
            fullPhoneContainer.SetActive(false);

        if (phoneMiniRect != null)
            phoneMiniRect.gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ClosePhone();
        else
            OpenPhone();
    }

    public void OpenPhone()
    {
        if (isPaused) return;
        isPaused = true;

        Time.timeScale = 0f;
        fullPhoneContainer.SetActive(true);
        if (phoneMiniRect != null) phoneMiniRect.gameObject.SetActive(false);

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimatePhone(fromMiniToFull: true));
    }

    public void ClosePhone()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimatePhone(fromMiniToFull: false));
    }

    private IEnumerator AnimatePhone(bool fromMiniToFull)
    {
        float elapsed = 0f;

        Vector2 targetMiniPos = phoneMiniRect != null
            ? (Vector2)phoneFullRect.parent.InverseTransformPoint(phoneMiniRect.position)
            : new Vector2(500f, 300f);

        Vector2 startPos = fromMiniToFull ? targetMiniPos : initialCenterAnchoredPos;
        Vector2 endPos = fromMiniToFull ? initialCenterAnchoredPos : targetMiniPos;

        Vector3 miniScale = new Vector3(0.2f, 0.2f, 1f);
        Vector3 startScale = fromMiniToFull ? miniScale : fullScale;
        Vector3 endScale = fromMiniToFull ? fullScale : miniScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveT = transitionCurve.Evaluate(t);

            phoneFullRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveT);
            phoneFullRect.localScale = Vector3.Lerp(startScale, endScale, curveT);

            yield return null;
        }

        phoneFullRect.anchoredPosition = endPos;
        phoneFullRect.localScale = endScale;

        if (!fromMiniToFull)
        {
            fullPhoneContainer.SetActive(false);
            if (phoneMiniRect != null) phoneMiniRect.gameObject.SetActive(true);
        }

        transitionCoroutine = null;
    }

    public void AppResumeGame()
    {
        ClosePhone();
    }

    public void AppGoToStreet()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(streetSceneName);
    }

    public void AppGoToSettings()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(settingsSceneName);
    }
}