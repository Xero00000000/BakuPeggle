using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class TemporaryManager : MonoBehaviour
{
    private bool player1Shot = false;
    private bool player2Shot = false;

    public bool ball1Destroyed = false;
    public bool ball2Destroyed = false;

    [SerializeField] private bool isPlayer2Turn;

    [Header("Habilidad & Indicador")]
    [SerializeField] private int abilityCharge;
    [SerializeField] private TextMeshProUGUI abilityChargeBar;
    [SerializeField] private int pointsToActivate = 100;
    [SerializeField] private List<RectTransform> imagesToAnimate;
    [SerializeField] private float bounceScale = 1.2f;
    [SerializeField] private float animSpeed = 6f;

    [Range(0f, 1f)][SerializeField] private float minOpacity = 0.3f;
    [Range(0f, 1f)][SerializeField] private float maxOpacity = 1.0f;

    private bool animActive = false;
    private Coroutine animCoroutine;
    private List<Vector3> originalScal = new List<Vector3>();

    [Header("Salud de Jugadores")]
    [SerializeField] private int player1MaxHP;
    [SerializeField] private int player2MaxHP;
    [SerializeField] private int player1CurrentHP;
    [SerializeField] private int player2CurrentHP;
    private int player1DamageToTake;
    private int player2DamageToTake;

    [Header("UI Barras de Vida Jugadores")]
    [SerializeField] private PlayerHealthUI player1HealthUI;
    [SerializeField] private PlayerHealthUI player2HealthUI;

    [Header("Efecto de Daño HLSL (Pantalla Completa)")]
    [SerializeField] private DamageEffectManager damageEffectManager;

    [Header("Event Channels")]
    [SerializeField] private ShotEventChannel _shoot;
    [SerializeField] private VoidEventChannel _newTurn;

    [Header("Configuración del Enemigo")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private float force;
    [SerializeField] private float spawnPointMinRotation;
    [SerializeField] private float spawnPointMaxRotation;

    [Header("Canvas")]
    [Tooltip("Canvas Win.")]
    [SerializeField] private GameObject winCanvas;

    [Tooltip("Canvas Lose")]
    [SerializeField] private GameObject loseCanvas;

    [Header("UI Puntos / Daño Jugador 1")]
    [SerializeField] private GameObject player1PointsCanvas;
    [SerializeField] private TextMeshProUGUI player1PointsText;

    [Header("UI Puntos / Daño Jugador 2")]
    [SerializeField] private GameObject player2PointsCanvas;
    [SerializeField] private TextMeshProUGUI player2PointsText;

    [Header("Configuración UI Puntos")]
    [Tooltip("Tiempo en segundos antes de ocultar los textos de puntos.")]
    [SerializeField] private float pointsDisplayDuration = 2f;

    private Coroutine hideP1Coroutine;
    private Coroutine hideP2Coroutine;
    private bool isGameOver = false;

    [Header("Configuración de Habilidad Especial")]
    [SerializeField] private float abilityInDuration = 0.12f;
    [SerializeField] private float abilityWaitTime = 0.5f;
    [SerializeField] private float abilityWaitToUnfreeze = 0.2f;
    [SerializeField] private List<RectTransform> abilityImageTransforms;

    [Header("Sonidos de Habilidad")]
    [SerializeField] private AudioClip abilityInSound;
    [SerializeField] private AudioClip hitSound;

    [Header("Shake de UI del Enemigo")]
    [Tooltip("Elementos de la UI del enemigo que recibirán el impacto (Barra de vida, avatar, etc.)")]
    [SerializeField] private List<RectTransform> enemyUIElementsToShake;
    [SerializeField] private float shakeIntensity = 15f;
    [SerializeField] private float shakeDuration = 0.3f;

    private bool isAbilityOn = false;

    public void Start()
    {
        player1CurrentHP = player1MaxHP;
        player2CurrentHP = player2MaxHP;
        player1DamageToTake = 0;
        player2DamageToTake = 0;

        // Guardar escalas base de las imágenes indicadoras
        originalScal.Clear();
        foreach (var img in imagesToAnimate)
        {
            if (img != null) originalScal.Add(img.localScale);
            else originalScal.Add(Vector3.one);
        }

        UpdateHealthUI();
        if (player1HealthUI != null)
        {
            player1HealthUI.ShowHealthText();
        }

        if (player2HealthUI != null)
        {
            player2HealthUI.ShowHealthText();
        }

        abilityCharge = 0;
        if (abilityChargeBar != null) abilityChargeBar.text = abilityCharge.ToString();

        if (winCanvas != null) winCanvas.SetActive(false);
        if (loseCanvas != null) loseCanvas.SetActive(false);

        if (player1PointsCanvas != null) player1PointsCanvas.SetActive(false);
        if (player2PointsCanvas != null) player2PointsCanvas.SetActive(false);

        ResetTurn();

        foreach (var img in abilityImageTransforms)
        {
            if (img != null) img.gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        if (isGameOver) return;

        if (player1Shot && player2Shot && ball1Destroyed && ball2Destroyed)
        {
            ResetTurn();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !player1Shot && abilityCharge >= pointsToActivate && !isAbilityOn)
        {
            StartCoroutine(AbilityAnimSetActive());
        }
    }

    private void ResetTurn()
    {
        if ((player1DamageToTake > 0 || player2DamageToTake > 0) && damageEffectManager != null)
        {
            damageEffectManager.TriggerDamageEffect();
        }

        player1CurrentHP -= player1DamageToTake;
        player2CurrentHP -= player2DamageToTake;
        player1DamageToTake = 0;
        player2DamageToTake = 0;

        UpdateHealthUI();

        player1Shot = false;
        player2Shot = false;
        ball1Destroyed = false;
        ball2Destroyed = false;

        if (player1CurrentHP <= 0 || player2CurrentHP <= 0)
        {
            HandleGameOver();
        }
        else
        {
            _newTurn.Raise(this);
            isPlayer2Turn = true;
        }
    }

    private void UpdateHealthUI()
    {
        if (player1HealthUI != null)
            player1HealthUI.UpdateHealth(player1CurrentHP, player1MaxHP);

        if (player2HealthUI != null)
            player2HealthUI.UpdateHealth(player2CurrentHP, player2MaxHP);
    }

    private void HandleGameOver()
    {
        isGameOver = true;

        if (player1PointsCanvas != null) player1PointsCanvas.SetActive(false);
        if (player2PointsCanvas != null) player2PointsCanvas.SetActive(false);

        if (player1CurrentHP > player2CurrentHP)
        {
            if (winCanvas != null) winCanvas.SetActive(true);
        }
        else
        {
            if (loseCanvas != null) loseCanvas.SetActive(true);
        }
    }

    public void WasShot(Component sender, int which)
    {
        if (isGameOver || isAbilityOn) return;

        if (which == 1)
        {
            player1Shot = true;
        }
        ShootEnemyBall();
    }

    public void AddPoints(Component sender, bool player, int type)
    {
        if (isGameOver) return;

        int points = 0;
        int charge = 0;
        switch (type)
        {
            case 0:
                points = 15;
                charge += 15;
                break;
            case 1:
                points = 30;
                charge += 30;
                break;
            case 2:
                points = 60;
                charge += 15;
                break;
        }

        if (player)
        {
            player2DamageToTake += points;
            ShowPlayerPoints(1, player2DamageToTake);
            abilityCharge += charge;
            if (abilityChargeBar != null)
                abilityChargeBar.text = abilityCharge.ToString();

            UpdateAbilityImageInd();
        }
        else
        {
            player1DamageToTake += points;
            ShowPlayerPoints(2, player1DamageToTake);
        }
    }

    private void UpdateAbilityImageInd()
    {
        if (abilityCharge >= pointsToActivate && !animActive)
        {
            StartAbilityAnim();
        }
        else if (abilityCharge < pointsToActivate && animActive)
        {
            StopAbilitiAnim();
        }
    }

    private void StartAbilityAnim()
    {
        if (imagesToAnimate == null || imagesToAnimate.Count == 0) return;

        animActive = true;
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimAbiliti());
    }

    private void StopAbilitiAnim()
    {
        animActive = false;
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }

        for (int i = 0; i < imagesToAnimate.Count; i++)
        {
            if (imagesToAnimate[i] != null)
            {
                if (i < originalScal.Count)
                    imagesToAnimate[i].localScale = originalScal[i];

                ChangeImageOpacity(imagesToAnimate[i], 1.0f);
            }
        }
    }

    private IEnumerator AnimAbiliti()
    {
        while (animActive)
        {
            float animMotion = (Mathf.Sin(Time.unscaledTime * animSpeed) + 1f) * 0.5f;
            float imagesScale = Mathf.Lerp(1f, bounceScale, animMotion);
            float currentOpacity = Mathf.Lerp(minOpacity, maxOpacity, animMotion);

            for (int i = 0; i < imagesToAnimate.Count; i++)
            {
                if (imagesToAnimate[i] != null)
                {
                    if (i < originalScal.Count)
                        imagesToAnimate[i].localScale = originalScal[i] * imagesScale;

                    ChangeImageOpacity(imagesToAnimate[i], currentOpacity);
                }
            }

            yield return null;
        }
    }
    private void ChangeImageOpacity(RectTransform rectTransform, float opacity)
    {
        if (rectTransform == null) return;

        CanvasGroup imageGroup = rectTransform.GetComponent<CanvasGroup>();
        if (imageGroup != null)
        {
            imageGroup.alpha = opacity;
            return;
        }

        Graphic image = rectTransform.GetComponent<Graphic>();
        if (image != null)
        {
            Color c = image.color;
            c.a = opacity;
            image.color = c;
        }
    }

    private void ShowPlayerPoints(int playerNumber, int points)
    {
        if (playerNumber == 1)
        {
            if (player1PointsCanvas == null) return;

            if (player1PointsText != null)
                player1PointsText.text = "+" + points.ToString() + " PTS";

            player1PointsCanvas.SetActive(true);

            if (hideP1Coroutine != null) StopCoroutine(hideP1Coroutine);
            hideP1Coroutine = StartCoroutine(HideRoutine(player1PointsCanvas));
        }
        else if (playerNumber == 2)
        {
            if (player2PointsCanvas == null) return;

            if (player2PointsText != null)
                player2PointsText.text = "+" + points.ToString() + " PTS";

            player2PointsCanvas.SetActive(true);

            if (hideP2Coroutine != null) StopCoroutine(hideP2Coroutine);
            hideP2Coroutine = StartCoroutine(HideRoutine(player2PointsCanvas));
        }
    }

    private IEnumerator HideRoutine(GameObject targetCanvas)
    {
        yield return new WaitForSeconds(pointsDisplayDuration);
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false);
        }
    }

    public void ShootEnemyBall()
    {
        if (isGameOver || isAbilityOn) return;

        float randomZ = Random.Range(spawnPointMinRotation, spawnPointMaxRotation);
        spawnPoint.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);
        var instance = Instantiate(ballPrefab, spawnPoint.transform.position, Quaternion.identity);
        instance.GetComponent<Rigidbody2D>().AddForce(spawnPoint.transform.up * force);
        player2Shot = true;
    }

    public IEnumerator AbilityAnimSetActive()
    {
        isAbilityOn = true;
        Time.timeScale = 0f;

        abilityCharge -= pointsToActivate;
        if (abilityChargeBar != null)
            abilityChargeBar.text = abilityCharge.ToString();

        UpdateAbilityImageInd();

        PlaySoundUnscaled(abilityInSound);

        yield return StartCoroutine(AnimateAbilityImages());
        yield return new WaitForSecondsRealtime(abilityWaitTime);
        yield return StartCoroutine(AnimateAbilityImagesOut());

        PlaySoundUnscaled(hitSound);

        if (damageEffectManager != null)
        {
            damageEffectManager.TriggerDamageEffect();
        }

        player2CurrentHP -= 100;
        UpdateHealthUI();

        yield return StartCoroutine(ShakeEnemyUI());
        yield return new WaitForSecondsRealtime(abilityWaitToUnfreeze);

        Time.timeScale = 1f;
        isAbilityOn = false;
    }

    private IEnumerator AnimateAbilityImages()
    {
        float elapsed = 0f;
        float screenWidth = Screen.width;

        foreach (var imgRect in abilityImageTransforms)
        {
            if (imgRect == null) continue;
            imgRect.gameObject.SetActive(true);

            CanvasGroup group = imgRect.GetComponent<CanvasGroup>();
            if (group == null) group = imgRect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;
        }

        while (elapsed < abilityInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / abilityInDuration);

            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            foreach (var imgRect in abilityImageTransforms)
            {
                if (imgRect == null) continue;
                float currentX = Mathf.Lerp(-screenWidth, 0f, easeT);
                imgRect.anchoredPosition = new Vector2(currentX, imgRect.anchoredPosition.y);
            }

            yield return null;
        }

        foreach (var imgRect in abilityImageTransforms)
        {
            if (imgRect != null)
                imgRect.anchoredPosition = new Vector2(0f, imgRect.anchoredPosition.y);
        }
    }

    private IEnumerator AnimateAbilityImagesOut()
    {
        float fadeDuration = 0.15f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (var imgRect in abilityImageTransforms)
            {
                if (imgRect == null) continue;
                CanvasGroup group = imgRect.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = alpha;
            }

            yield return null;
        }

        foreach (var imgRect in abilityImageTransforms)
        {
            if (imgRect != null) imgRect.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShakeEnemyUI()
    {
        if (enemyUIElementsToShake == null || enemyUIElementsToShake.Count == 0) yield break;

        List<Vector2> originalPositions = new List<Vector2>();
        foreach (var uiElem in enemyUIElementsToShake)
        {
            if (uiElem != null)
                originalPositions.Add(uiElem.anchoredPosition);
            else
                originalPositions.Add(Vector2.zero);
        }

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            for (int i = 0; i < enemyUIElementsToShake.Count; i++)
            {
                RectTransform uiElem = enemyUIElementsToShake[i];
                if (uiElem == null) continue;

                Vector2 offset = Random.insideUnitCircle * shakeIntensity;
                uiElem.anchoredPosition = originalPositions[i] + offset;
            }

            yield return null;
        }

        for (int i = 0; i < enemyUIElementsToShake.Count; i++)
        {
            if (enemyUIElementsToShake[i] != null)
                enemyUIElementsToShake[i].anchoredPosition = originalPositions[i];
        }
    }

    private void PlaySoundUnscaled(AudioClip clip)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("TempAbilityAudio");
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.ignoreListenerPause = true;
        audioSource.Play();

        Destroy(soundObj, clip.length);
    }
}