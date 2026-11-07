using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PeggleUIManager : MonoBehaviour
{
    private int player1CurrentHP;
    private int player1MaxHP;
    private int player2CurrentHP;
    private int player2MaxHP;
    private int player1DamageToTake;
    private int player2DamageToTake;

    [SerializeField] private bool isDamageAbilityActive;

    [Header("Event Channels")]
    [SerializeField] private ShotEventChannel Continue;

    [Header("Habilidad & Indicador")]
    [SerializeField] private int abilityCharge;
    [SerializeField] private TextMeshProUGUI abilityChargeBar;
    [SerializeField] private List<RectTransform> imagesToAnimate;
    [SerializeField] private float bounceScale = 1.2f;
    [SerializeField] private float animSpeed = 6f;

    [Header("Barra de Habilidad")]
    [SerializeField] private Graphic abilityBar;
    [Tooltip("Color base de la barra por habilidad: 0 Damage, 1 Heal, 2 Duplicate, 3 Bomb")]
    [SerializeField]
    private Color[] abilityBarColors = new Color[]
    {
    new Color(0.85f, 0.15f, 0.15f),   // 0 damage
    new Color(0.15f, 0.7f, 0.2f),     // 1 heal
    new Color(0.2f, 0.6f, 0.9f),      // 2 duplicate
    new Color(0.9f, 0.6f, 0.1f)       // 3 bomb
    };
    [SerializeField, Range(0f, 1f)] private float lightenAmount = 0.5f;
    [SerializeField] private float barFadeSpeed = 8f;
    [SerializeField] private Color abilityBarDefaultColor = new Color(0.15f, 0.7f, 0.2f);   // your normal green

    private Color barNormalColor;
    private Color barTargetColor;
    private bool lastCanUse;
    private int lastAbilityIndex = -1;

    [Range(0f, 1f)] [SerializeField] private float minOpacity = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float maxOpacity = 1.0f;

    private bool animActive = false;
    private Coroutine animCoroutine;
    private List<Vector3> originalScal = new List<Vector3>();

    [Header("UI Barras de Vida Jugadores")]
    [SerializeField] private PlayerHealthUI player1HealthUI;
    [SerializeField] private PlayerHealthUI player2HealthUI;

    [Header("Efecto de Daño HLSL (Pantalla Completa)")]
    [SerializeField] private DamageEffectManager damageEffectManager;

    [Header("Canvas de Juego (Win/Lose)")]
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;

    [Header("UI Puntos")]
    [SerializeField] private GameObject player1PointsCanvas;
    [SerializeField] private TextMeshProUGUI player1PointsText;
    [SerializeField] private GameObject player2PointsCanvas;
    [SerializeField] private TextMeshProUGUI player2PointsText;
    [SerializeField] private float pointsDisplayDuration = 2f;

    [Header("Floating Text")]
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Color playerTextColor = Color.white;
    [SerializeField] private Color enemyTextColor = Color.red;
    private FloatingText activePlayerText;
    private FloatingText activeEnemyText;

    [Header("Daño Total (reemplaza los canvas de recuento)")]
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Color playerDamageColor = Color.white;   // damage the player deals
    [SerializeField] private Color enemyDamageColor = Color.red;      // damage the player receives
    [Tooltip("Opcional. Si está vacío se usa el transform del Player Health UI.")]
    [SerializeField] private Transform player1HealthTarget;
    [SerializeField] private Transform player2HealthTarget;
    [SerializeField] private float holdBeforeFly = 0.6f;
    [SerializeField] private DamageTextArea playerBoardArea;   // board where the player's ball lands
    [SerializeField] private DamageTextArea enemyBoardArea;

    private DamageNumber pendingToEnemy;    // total the player deals
    private DamageNumber pendingToPlayer;   // total the enemy deals
    private int arrivalsPending;

    [Header("Combo: color coincidente")]
    [Tooltip("Tipo de peg que coincide con cada bola. Índice = habilidad (0 Damage, 1 Heal, 2 Duplicate, 3 Bomb). -1 = ninguno.")]
    [SerializeField] private int[] abilityToPegType = new int[] { 0, 1, 2, -1 };

    [Header("Configuración de Turnos y Tiempos")]
    [SerializeField] private GameObject turnNoticeCanvas;
    [SerializeField] private TextMeshProUGUI turnNoticeText;
    [SerializeField] private List<RectTransform> turnNoticeElements;
    [SerializeField] private float initialTurnDelay = 1.5f;
    [SerializeField] private float turnNoticeDuration = 1.5f;
   
    [SerializeField] private float delayBeforeTurnNotice = 1.0f;
    [SerializeField] private MonoBehaviour scriptToDisable;

    [Header("Testeo de Animaciones del Cartel (Inspector)")]
    [Range(-1, 5)] [SerializeField] private int forcedAnimStyle = -1;

    private Coroutine hideP1Coroutine;
    private Coroutine hideP2Coroutine;
    private bool isGameOver = false;
    private bool isFirstTurn = true;
    private bool ignoreShootThisFrame = false;
    private bool isTurnNoticeRunning = false;
    private bool isEndingTurn = false;


    [Header("Configuración de Habilidad Especial")]
    [SerializeField] private float abilityInDuration = 0.12f;
    [SerializeField] private float abilityWaitTime = 0.5f;
    [SerializeField] private float abilityWaitToUnfreeze = 0.2f;
    [SerializeField] private List<RectTransform> abilityImageTransforms;

    [Header("Sonidos y Shake")]
    [SerializeField] private AudioClip abilityInSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private List<RectTransform> enemyUIElementsToShake;
    [SerializeField] private float shakeIntensity = 15f;
    [SerializeField] private float shakeDuration = 0.3f;

    [Header("Referencias")]
    [SerializeField] private TurnManager turnManager;

    private bool isAbilityOn = false;

    
    private List<Vector2> turnNoticeOriginalPos = new List<Vector2>();


    private void Awake()
    {
        if (abilityBar != null)
        {
            barNormalColor = GetBarBaseColor();
            barTargetColor = barNormalColor;
            abilityBar.color = barNormalColor;
        }
    }

    private Color GetBarBaseColor()
    {
        return abilityBarDefaultColor;
    }

    private Color GetAbilityColor()
    {
        int idx = turnManager != null ? turnManager.CurrentAbility : 0;
        if (abilityBarColors != null && idx >= 0 && idx < abilityBarColors.Length)
            return abilityBarColors[idx];
        return abilityBarDefaultColor;
    }

    private void Update()
    {
        if (abilityBar != null)
            abilityBar.color = Color.Lerp(abilityBar.color, barTargetColor, Time.unscaledDeltaTime * barFadeSpeed);

        bool canUse = turnManager != null && turnManager.PlayerCanAct
                      && abilityCharge >= turnManager.PointsToActivate;
        int abilityIndex = turnManager != null ? turnManager.CurrentAbility : 0;

        // Refresh when the turn state changes or the player switches ball
        if (canUse != lastCanUse || abilityIndex != lastAbilityIndex)
        {
            lastCanUse = canUse;
            lastAbilityIndex = abilityIndex;
            UpdateAbilityImageInd();
        }
    }

    public void AnimatingUI(Component turnManager, int[] variables)
    {
        player1CurrentHP = variables[1];
        player1MaxHP = variables[2];
        player2CurrentHP = variables[3];
        player2MaxHP = variables[4];
        player1DamageToTake = variables[5];
        player2DamageToTake = variables[6];
        abilityCharge = variables[7];

        switch (variables[0])
        {
            case 0: //first turn
                StartSetUp();
                break;
            case 1: //other turns
                StartCoroutine(EndTurnRoutine());
                Debug.Log("eeeeeh");
                break; 
            case 2: //ability use
                Debug.Log("ooooooh");
                StartCoroutine(AbilityAnimSetActive());
                break;
            case 3: //reset turn
                Debug.Log("uuuuuuh");
                ShowTurnNoticeRoutine();
                break;
        }

        Debug.Log("aaaaah");
    }

    private void StartSetUp()
    {
        originalScal.Clear();
        foreach (var img in imagesToAnimate)
        {
            if (img != null)
            {
                originalScal.Add(img.localScale);
                img.gameObject.SetActive(false);
            }
            else
            {
                originalScal.Add(Vector3.one);
            }
        }

        

        turnNoticeOriginalPos.Clear();
        foreach (var rt in turnNoticeElements)
        {
            if (rt != null) turnNoticeOriginalPos.Add(rt.anchoredPosition);
        }

        UpdateHealthUI();
        if (player1HealthUI != null) player1HealthUI.ShowHealthText();
        if (player2HealthUI != null) player2HealthUI.ShowHealthText();

        //
        if (abilityChargeBar != null) abilityChargeBar.text = abilityCharge.ToString();

        if (winCanvas != null) winCanvas.SetActive(false);
        if (loseCanvas != null) loseCanvas.SetActive(false);
        if (player1PointsCanvas != null) player1PointsCanvas.SetActive(false);
        if (player2PointsCanvas != null) player2PointsCanvas.SetActive(false);
        if (turnNoticeCanvas != null) turnNoticeCanvas.SetActive(false);
       

        foreach (var img in abilityImageTransforms)
        {
            if (img != null) img.gameObject.SetActive(false);
        }

        UpdateAbilityImageInd();
    }

    private IEnumerator ShowTurnNoticeRoutine()
    {
        isTurnNoticeRunning = true;

        if (turnNoticeCanvas != null)
        {
            if (isFirstTurn)
            {
                yield return new WaitForSeconds(initialTurnDelay);
                isFirstTurn = false;
            }

            ignoreShootThisFrame = true;
            if (scriptToDisable != null) scriptToDisable.enabled = false;

            turnNoticeCanvas.SetActive(true);
            Time.timeScale = 0f;

            CanvasGroup canvasGroup = turnNoticeCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = turnNoticeCanvas.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;

            int animStyle = (forcedAnimStyle >= 0 && forcedAnimStyle <= 5) ? forcedAnimStyle : Random.Range(0, 6);

            float elapsed = 0f;
            float enterDuration = 0.3f;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            while (elapsed < enterDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / enterDuration);

                for (int i = 0; i < turnNoticeElements.Count; i++)
                {
                    RectTransform rt = turnNoticeElements[i];
                    if (rt == null) continue;

                    Vector2 origPos = i < turnNoticeOriginalPos.Count ? turnNoticeOriginalPos[i] : rt.anchoredPosition;

                    switch (animStyle)
                    {
                        case 0:
                            rt.anchoredPosition = Vector2.Lerp(origPos + Vector2.left * screenWidth, origPos, t);
                            break;
                        case 1:
                            rt.anchoredPosition = Vector2.Lerp(origPos + Vector2.right * screenWidth, origPos, t);
                            break;
                        case 2:
                            rt.localRotation = Quaternion.Euler(Mathf.Lerp(360f, 0f, t), 0f, 0f);
                            break;
                        case 3:
                            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                            break;
                        case 4:
                            rt.anchoredPosition = Vector2.Lerp(origPos + Vector2.up * screenHeight, origPos, t);
                            break;
                        case 5:
                            rt.anchoredPosition = Vector2.Lerp(origPos + Vector2.down * screenHeight, origPos, t);
                            break;
                    }
                }
                yield return null;
            }

            for (int i = 0; i < turnNoticeElements.Count; i++)
            {
                if (turnNoticeElements[i] != null && i < turnNoticeOriginalPos.Count)
                {
                    turnNoticeElements[i].anchoredPosition = turnNoticeOriginalPos[i];
                    turnNoticeElements[i].localRotation = Quaternion.identity;
                }
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(turnNoticeDuration);

            float fadeElapsed = 0f;
            float fadeDuration = 0.4f;

            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(fadeElapsed / fadeDuration);

                for (int i = 0; i < turnNoticeElements.Count; i++)
                {
                    RectTransform rt = turnNoticeElements[i];
                    if (rt == null) continue;

                    Vector2 origPos = i < turnNoticeOriginalPos.Count ? turnNoticeOriginalPos[i] : rt.anchoredPosition;

                    switch (animStyle)
                    {
                        case 0:
                            rt.anchoredPosition = Vector2.Lerp(origPos, origPos + Vector2.right * screenWidth, t);
                            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                            break;
                        case 1:
                            rt.anchoredPosition = Vector2.Lerp(origPos, origPos + Vector2.left * screenWidth, t);
                            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                            break;
                        case 2:
                            rt.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 360f, t), 0f, 0f);
                            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                            break;
                        case 3:
                            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                            break;
                        case 4:
                            rt.anchoredPosition = Vector2.Lerp(origPos, origPos + Vector2.up * screenHeight, t);
                            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                            break;
                        case 5:
                            rt.anchoredPosition = Vector2.Lerp(origPos, origPos + Vector2.down * screenHeight, t);
                            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                            break;
                    }
                }
                yield return null;
            }

            turnNoticeCanvas.SetActive(false);

            for (int i = 0; i < turnNoticeElements.Count; i++)
            {
                if (turnNoticeElements[i] != null && i < turnNoticeOriginalPos.Count)
                {
                    turnNoticeElements[i].anchoredPosition = turnNoticeOriginalPos[i];
                    turnNoticeElements[i].localRotation = Quaternion.identity;
                }
            }
            canvasGroup.alpha = 1f;

            Time.timeScale = 1f;

            if (scriptToDisable != null) scriptToDisable.enabled = true;

            yield return null;
            ignoreShootThisFrame = false;
        }

        isTurnNoticeRunning = false;
    }

    private IEnumerator EndTurnRoutine()
    {
        // Make sure the numbers show the final totals
        if (pendingToEnemy != null) pendingToEnemy.SetText(player2DamageToTake.ToString());
        if (pendingToPlayer != null) pendingToPlayer.SetText(player1DamageToTake.ToString());

        if (pendingToEnemy != null || pendingToPlayer != null)
            yield return new WaitForSeconds(holdBeforeFly);   // let the player read them

        arrivalsPending = 0;
        if (pendingToEnemy != null) FlyToHealth(pendingToEnemy, true);
        if (pendingToPlayer != null) FlyToHealth(pendingToPlayer, false);

        float timeout = 3f;
        while (arrivalsPending > 0 && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        pendingToEnemy = null;
        pendingToPlayer = null;
        UpdateHealthUI();
        FeedbackBalls.ResetCombos();   // new turn, fresh streaks

        if (player1CurrentHP <= 0 || player2CurrentHP <= 0)
        {
            HandleGameOver();
        }
        else
        {
            yield return new WaitForSeconds(delayBeforeTurnNotice);
            Continue.Raise(this, 1);
        }
    }

    public void ShowTotalDamage(bool fromPlayer, Vector3 worldPos)
    {
        if (isGameOver || damageNumberPrefab == null) return;

        int damage = fromPlayer ? player2DamageToTake : player1DamageToTake;
        if (damage <= 0) return;

        worldPos = ClampToScreen(worldPos);

        float size = Mathf.Lerp(1f, 1.8f, Mathf.InverseLerp(30f, 300f, damage));

        ref DamageNumber slot = ref (fromPlayer ? ref pendingToEnemy : ref pendingToPlayer);

        if (slot == null)
            slot = Instantiate(damageNumberPrefab, worldPos, Quaternion.identity);
        else
            slot.transform.position = worldPos;

        slot.Show(damage.ToString(), fromPlayer ? playerDamageColor : enemyDamageColor, size);


        DamageTextArea area = fromPlayer ? playerBoardArea : enemyBoardArea;
        if (area != null) slot.KeepInsideArea(area.WorldRect);

        slot.BringToFront(Camera.main);



    }

    private void FlyToHealth(DamageNumber num, bool toEnemy)
    {
        PlayerHealthUI ui = toEnemy ? player2HealthUI : player1HealthUI;
        Transform target = toEnemy ? player2HealthTarget : player1HealthTarget;
        if (target == null && ui != null) target = ui.transform;

        if (target == null)
        {
            Destroy(num.gameObject);
            return;
        }

        Vector3 dest = GetTargetWorldPos(target, num.transform.position);
        arrivalsPending++;

        num.FlyTo(dest, () =>
        {
            if (ui != null)
            {
                if (toEnemy) ui.UpdateHealth(player2CurrentHP, player2MaxHP);
                else ui.UpdateHealth(player1CurrentHP, player1MaxHP);
            }
            if (damageEffectManager != null) damageEffectManager.TriggerDamageEffect();
            arrivalsPending--;
        });
    }

    // Works for world objects and for UI elements (overlay, camera or world canvas)
    private Vector3 GetTargetWorldPos(Transform target, Vector3 referenceWorldPos)
    {
        Camera cam = Camera.main;
        Canvas canvas = target.GetComponentInParent<Canvas>();
        if (canvas == null) return target.position;

        canvas = canvas.rootCanvas;
        if (canvas.renderMode == RenderMode.WorldSpace) return target.position;

        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, target.position);
        float depth = cam.WorldToScreenPoint(referenceWorldPos).z;
        return cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
    }

    // The ball lands at the very bottom, so keep the number inside the visible area
    private Vector3 ClampToScreen(Vector3 worldPos)
    {
        Camera cam = Camera.main;
        Vector3 v = cam.WorldToViewportPoint(worldPos);
        v.x = Mathf.Clamp(v.x, 0.1f, 0.9f);
        v.y = Mathf.Clamp(v.y, 0.15f, 0.9f);
        return cam.ViewportToWorldPoint(v);
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
        if (turnNoticeCanvas != null) turnNoticeCanvas.SetActive(false);
        

        if (player1CurrentHP > player2CurrentHP)
        {
            if (winCanvas != null) winCanvas.SetActive(true);
        }
        else
        {
            if (loseCanvas != null) loseCanvas.SetActive(true);
        }

        Continue.Raise(this, 0);   // NEW: tells TurnManager the game is over
    }

    public void AddPoints(Component sender, bool player, int type)
    {
        if (isGameOver) return;

        int points = 0;
        int charge = 0;
        switch (type)
        {
            case 0: points = 15; charge += 15; break;
            case 1: points = 30; charge += 30; break;
            case 2: points = 60; charge += 15; break;
        }

        int runningTotal;
        if (player)
        {
            player2DamageToTake += points;
            runningTotal = player2DamageToTake;
            ShowPlayerPoints(1, runningTotal);
            abilityCharge += charge;
            if (abilityChargeBar != null)
                abilityChargeBar.text = abilityCharge.ToString();
            UpdateAbilityImageInd();
        }
        else
        {
            player1DamageToTake += points;
            runningTotal = player1DamageToTake;
            ShowPlayerPoints(2, runningTotal);
        }

        Color textColor = player ? playerTextColor : enemyTextColor;
        int streak = 1;
        bool matchesBall = false;

        if (sender is FeedbackBalls fb)
        {
            textColor = fb.TextColor;
            streak = fb.HitStreak;

            if (turnManager != null)
            {
                int ability = turnManager.CurrentAbility;
                if (abilityToPegType != null && ability >= 0 && ability < abilityToPegType.Length)
                    matchesBall = abilityToPegType[ability] == fb.PegType;
            }
        }

        string label = streak > 1 ? $"{runningTotal} (x{streak})" : runningTotal.ToString();
        SpawnFloatingText(sender.transform.position, label, textColor, player, matchesBall);
    }

    private void SpawnFloatingText(Vector3 pos, string label, Color color, bool player, bool boosted)
    {
        if (floatingTextPrefab == null) return;

        ref FloatingText slot = ref (player ? ref activePlayerText : ref activeEnemyText);

        if (slot == null)
            slot = Instantiate(floatingTextPrefab, pos, Quaternion.identity);
        else
            slot.transform.position = pos;

        slot.Setup(label, color, boosted);
    }

    private void UpdateAbilityImageInd()
    {
        int required = turnManager != null ? turnManager.PointsToActivate : int.MaxValue;
        bool hasCharge = abilityCharge >= required;
        bool canUse = hasCharge && turnManager != null && turnManager.PlayerCanAct;

        
        // Default color while charging, the ball's color (lightened) once the ability can be used
        barNormalColor = abilityBarDefaultColor;
        barTargetColor = hasCharge
            ? Color.Lerp(GetAbilityColor(), Color.white, lightenAmount)
            : abilityBarDefaultColor;

        if (canUse && !animActive)
        {
            StartAbilityAnim();
        }
        else if (!canUse && animActive)
        {
            StopAbilitiAnim();
        }
        else if (!canUse)
        {
            foreach (var img in imagesToAnimate)
            {
                if (img != null && img.gameObject.activeSelf)
                    img.gameObject.SetActive(false);
            }
        }
    }

    private void StartAbilityAnim()
    {
        if (imagesToAnimate == null || imagesToAnimate.Count == 0) return;

        animActive = true;
        foreach (var img in imagesToAnimate)
        {
            if (img != null) img.gameObject.SetActive(true);
        }

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
                imagesToAnimate[i].gameObject.SetActive(false);
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
            if (player1PointsText != null) player1PointsText.text = "+" + points.ToString() + " PTS";
            player1PointsCanvas.SetActive(true);

            if (hideP1Coroutine != null) StopCoroutine(hideP1Coroutine);
            hideP1Coroutine = StartCoroutine(HideRoutine(player1PointsCanvas));
        }
        else if (playerNumber == 2)
        {
            if (player2PointsCanvas == null) return;
            if (player2PointsText != null) player2PointsText.text = "+" + points.ToString() + " PTS";
            player2PointsCanvas.SetActive(true);

            if (hideP2Coroutine != null) StopCoroutine(hideP2Coroutine);
            hideP2Coroutine = StartCoroutine(HideRoutine(player2PointsCanvas));
        }
    }

    private IEnumerator HideRoutine(GameObject targetCanvas)
    {
        yield return new WaitForSeconds(pointsDisplayDuration);
        if (targetCanvas != null) targetCanvas.SetActive(false);
    }

    public IEnumerator AbilityAnimSetActive()
    {
        isAbilityOn = true;
        Time.timeScale = 0f;

        // Resta la energía requerida por la habilidad activa (150 o 300)
        if (abilityChargeBar != null)
            abilityChargeBar.text = abilityCharge.ToString();

        UpdateAbilityImageInd();
        PlaySoundUnscaled(abilityInSound);

        yield return StartCoroutine(AnimateAbilityImages());
        yield return new WaitForSecondsRealtime(abilityWaitTime);
        yield return StartCoroutine(AnimateAbilityImagesOut());

        PlaySoundUnscaled(hitSound);

        if (damageEffectManager != null) damageEffectManager.TriggerDamageEffect();

        UpdateHealthUI();

        yield return StartCoroutine(ShakeEnemyUI());
        yield return new WaitForSecondsRealtime(abilityWaitToUnfreeze);

        Time.timeScale = 1f;
        isAbilityOn = false;

        Continue.Raise(this, 2);
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
            imgRect.anchoredPosition = new Vector2(0f, imgRect.anchoredPosition.y);
        }
    }

    public void SetDamageAbility()
    {
        isDamageAbilityActive = true;
        UpdateAbilityImageInd();
    }

    public void SetHealAbility()
    {
        isDamageAbilityActive = false;
        UpdateAbilityImageInd();
    }
    public void ToggleAbility()
    {
        isDamageAbilityActive = !isDamageAbilityActive;
        UpdateAbilityImageInd();
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
            if (uiElem != null) originalPositions.Add(uiElem.anchoredPosition);
            else originalPositions.Add(Vector2.zero);
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
