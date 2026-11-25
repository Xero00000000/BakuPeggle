using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private GameObject persistentBullshit;
    [SerializeField] private GameObject bombShit;
    [SerializeField] private GameObject duplicateShit;

    [Header("Event Channels")]
    [SerializeField] private ShotEventChannel _shoot;
    [SerializeField] private VoidEventChannel _newTurn;
    [SerializeField] private TurnUpdateEventChannel _turnUpdateUI;

    private bool player1Shot = false;
    private bool player2Shot = false;

    public bool ball1Destroyed = false;
    public bool ball2Destroyed = false;
    public int maxBalls;
    public int currentBallsDestroyed;

    [SerializeField] private bool isPlayer2Turn;

    [Header("Selector de Habilidad Activa")]
    [Tooltip("0 = Daño | 1 = Curación | 2 = Clonar | 3 = Refrescar")]
    [SerializeField] private int currentAbility;

    [Header("Cut-in por Habilidad (al ejecutar)")]
    [Tooltip("Ej: cutin_0_A, cutin_0_B (elige uno al azar al usar habilidad 0)")]
    [SerializeField] private Image[] damageCutinImages;

    [Tooltip("Imágenes de cut-in al usar habilidad 1 (Curación)")]
    [SerializeField] private Image[] healCutinImages;

    [Tooltip("Imágenes de cut-in al usar habilidad 2 (Clonar)")]
    [SerializeField] private Image[] cloneCutinImages;

    [Tooltip("Imágenes de cut-in al usar habilidad 3 (Refrescar)")]
    [SerializeField] private Image[] refreshCutinImages;

    [Header("Configuración de Prefabs de Bolas")]
    [SerializeField] private GameObject damageBallPrefab;
    [SerializeField] private GameObject healBallPrefab;
    [SerializeField] private GameObject cloneBallPrefab;
    [SerializeField] private GameObject refreshBallPrefab;

    [Header("Configuración de Habilidades")]
    [SerializeField] private int damageCost = 150;
    [SerializeField] private int damageEffectValue = 100;

    [SerializeField] private int healCost = 300;
    [SerializeField] private int healEffectValue = 150;

    [SerializeField] private int multiplyCost = 150;

    [SerializeField] private int refreshCost = 300;
    [SerializeField] private BoxCollider refreshArea;
    [SerializeField] private GameObject pegPrefab;
    [SerializeField] private int refreshNumber;

    [Header("Recompensa")]
    [SerializeField] private int winReward = 100;
    [Header("Nivel")]
    [Tooltip("Number of this level (1, 2, 3...). Winning it unlocks the next one.")]
    [SerializeField] private int levelNumber = 1;

    [Header("Safety")]
    [Tooltip("If a turn takes longer than this after the player shoots, it is forced to end. 0 = disabled.")]
    [SerializeField] private float turnTimeout = 20f;
    private float player1ShotTime;

    private int pointsToActivate => currentAbility switch
    {
        0 => damageCost,
        1 => healCost,
        2 => multiplyCost,
        3 => refreshCost,
        _ => 0
    };

    public int PointsToActivate => pointsToActivate;
    public bool PlayerCanAct => !isGameOver && !isEndingTurn && !isAbilityOn && !player1Shot;

    public bool IsGameOver => isGameOver;
    public int CurrentAbility => currentAbility;
    private int effectValue => currentAbility switch
    {
        0 => damageEffectValue,
        1 => healEffectValue,
        _ => 0
    };

    [Header("Habilidad")]
    [SerializeField] private int abilityCharge;

    [Header("Salud y Daño de Jugadores")]
    [SerializeField] private int player1MaxHP;
    [SerializeField] private int player2MaxHP;
    [SerializeField] private int player1CurrentHP;
    [SerializeField] private int player2CurrentHP;
    private int player1DamageToTake;
    private int player2DamageToTake;

    [Header("Configuración del Enemigo")]
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private float force;
    [SerializeField] private Collider enemyAimArea;
    [SerializeField] private float spawnPointMinRotation;
    [SerializeField] private float spawnPointMaxRotation;
    [SerializeField] private float launchSpeed = 12f;

    [Header("Rotación del lanzador enemigo")]
    [SerializeField] private Transform enemyLauncher;
    [SerializeField] private float launcherAngleOffset = -90f;
    [SerializeField] private float launcherRotateDuration = 0.35f;
    [SerializeField] private float enemyMinAngle = -170f;
    [SerializeField] private float enemyMaxAngle = -10f;
    private float launcherAngle;
    private bool launcherAngleReady;
    private bool enemyShooting;

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

    private bool isAbilityOn = false;
    private Coroutine cutinRoutine;

    [SerializeField] private int[] variablesForUI;

    void Start()
    {
        persistentBullshit = GameObject.Find("PersistentBullshit");

        if (!persistentBullshit)
        {
            bombShit.SetActive(true);
            duplicateShit.SetActive(true);
        }
        else
        {
            if (persistentBullshit.GetComponent<PersistentBullshit>().bakugan1 == true)
            {
                bombShit.SetActive(true);
            }
            else
            {
                bombShit.SetActive(false);
            }
            if (persistentBullshit.GetComponent<PersistentBullshit>().bakugan2 == true)
            {
                duplicateShit.SetActive(true);
            }
            else
            {
                duplicateShit.SetActive(false);
            }
        }

        HideAllCutins();

        player1CurrentHP = player1MaxHP;
        player2CurrentHP = player2MaxHP;
        abilityCharge = 0;
        maxBalls = 1;
        currentBallsDestroyed = 0;

        variablesForUI = new int[] { 0, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge };
        _turnUpdateUI.Raise(this, variablesForUI);
    }

    void Update()
    {
        if (isGameOver || isEndingTurn) return;

        if (turnTimeout > 0f && player1Shot && Time.time - player1ShotTime > turnTimeout)
        {
            Debug.LogWarning("[TurnManager] Turn timed out, forcing it to end.");
            player2Shot = true;
            ball1Destroyed = true;
            ball2Destroyed = true;
            enemyShooting = false;
            StopAllCoroutines();
            Time.timeScale = 1f;
            isAbilityOn = false;
        }

        if (player1Shot && player2Shot && ball1Destroyed && ball2Destroyed)
        {
            isEndingTurn = true;
            EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !player1Shot && abilityCharge >= pointsToActivate && !isAbilityOn && !ignoreShootThisFrame)
        {
            UseAbility();
        }
    }

    private void ResetTurn()
    {
        player1Shot = false;
        player2Shot = false;
        ball1Destroyed = false;
        ball2Destroyed = false;
        isEndingTurn = false;
        enemyShooting = false;

        maxBalls = 1;
        currentBallsDestroyed = 0;
        player1DamageToTake = 0;
        player2DamageToTake = 0;

        _newTurn.Raise(this);
        isPlayer2Turn = true;

        variablesForUI = new int[] { 3, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge };
        _turnUpdateUI.Raise(this, variablesForUI);
    }

    public void Continue(Component that, int variable)
    {
        switch (variable)
        {
            case 0: HandleGameOver(); break;
            case 1: ResetTurn(); break;
            case 2: isAbilityOn = false; break;
        }
    }

    private void EndTurn()
    {
        Debug.Log("iiiiiii");
        if (player2DamageToTake > 0)
            player2CurrentHP -= player2DamageToTake;

        if (player1DamageToTake > 0)
            player1CurrentHP -= player1DamageToTake;

        variablesForUI = new int[] { 1, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge };

        _turnUpdateUI.Raise(this, variablesForUI);
    }

    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        bool playerWon = player1CurrentHP > player2CurrentHP;
        Debug.Log($"[Reward] Game over. P1={player1CurrentHP} P2={player2CurrentHP} won={playerWon}");

        if (playerWon)
        {
            MarbeastSave.AddCoins(winReward);
            LevelProgress.CompleteLevel(levelNumber);
        }

        variablesForUI = new int[] { 4, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge };
        _turnUpdateUI.Raise(this, variablesForUI);
    }

    public void WasShot(Component sender, int which)
    {
        if (isGameOver || isAbilityOn || ignoreShootThisFrame) return;

        if (which == 1)
        {
            player1Shot = true;
            player1ShotTime = Time.time;
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
            case 0: points = 15; charge += 15; break;
            case 1: points = 30; charge += 30; break;
            case 2: points = 60; charge += 15; break;
        }

        if (sender is FeedbackBalls hitPeg)
        {
            points = Mathf.RoundToInt(points * hitPeg.PointsMultiplier);
            charge = Mathf.RoundToInt(charge * hitPeg.PointsMultiplier);
        }

        if (player)
        {
            player2DamageToTake += points;
            abilityCharge += charge;
        }
        else
        {
            player1DamageToTake += points;
        }
    }

    public void ShootEnemyBall()
    {
        if (isGameOver || isAbilityOn || ignoreShootThisFrame) return;

        StartCoroutine(EnemyShootRoutine());
    }

    private IEnumerator EnemyShootRoutine()
    {
        if (enemyShooting) yield break;
        enemyShooting = true;

        GameObject prefabToSpawn = currentAbility switch
        {
            0 => damageBallPrefab,
            1 => healBallPrefab,
            2 => cloneBallPrefab,
            3 => refreshBallPrefab,
            _ => null
        };

        if (prefabToSpawn == null)
        {
            player2Shot = true;
            enemyShooting = false;
            yield break;
        }

        Vector2 origin = spawnPoint.transform.position;
        Vector2 vel;
        Vector2 aimDir;

        if (enemyAimArea != null)
        {
            Bounds b = enemyAimArea.bounds;
            Vector2 target = new Vector2(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y));

            var prefabRb = prefabToSpawn.GetComponent<Rigidbody2D>();
            float gravityScale = prefabRb != null ? prefabRb.gravityScale : 1f;

            vel = SolveLaunchVelocity(origin, target, gravityScale);
            aimDir = target - origin;
        }
        else
        {
            float randomZ = Random.Range(spawnPointMinRotation, spawnPointMaxRotation);
            vel = Quaternion.Euler(0f, 0f, randomZ) * Vector3.up * launchSpeed;
            aimDir = vel;
        }

        Transform pivot = enemyLauncher != null ? enemyLauncher : spawnPoint.transform;

        if (!launcherAngleReady)
        {
            launcherAngle = Mathf.DeltaAngle(0f, pivot.eulerAngles.z);
            launcherAngleReady = true;
        }

        float aimWorld = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        aimWorld = Mathf.Clamp(aimWorld, enemyMinAngle, enemyMaxAngle);
        float targetAngle = aimWorld + launcherAngleOffset;

        float startAngle = launcherAngle;

        if (launcherRotateDuration > 0f)
        {
            float t = 0f;
            while (t < launcherRotateDuration)
            {
                if (isGameOver) { enemyShooting = false; yield break; }

                t += Time.deltaTime;
                float eased = Mathf.SmoothStep(0f, 1f, t / launcherRotateDuration);
                launcherAngle = Mathf.Lerp(startAngle, targetAngle, eased);
                pivot.rotation = Quaternion.Euler(0f, 0f, launcherAngle);
                yield return null;
            }
        }

        launcherAngle = targetAngle;
        pivot.rotation = Quaternion.Euler(0f, 0f, launcherAngle);

        if (isGameOver) { enemyShooting = false; yield break; }

        var instance = Instantiate(prefabToSpawn, spawnPoint.transform.position, Quaternion.identity);

        var rb = instance.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = vel;
        else Debug.LogError($"Enemy prefab {prefabToSpawn.name} has no Rigidbody2D!");

        var glow = instance.GetComponent<BallComboGlow>();
        if (glow != null) glow.SetSide(false);

        player2Shot = true;
        enemyShooting = false;
    }

    private Vector2 SolveLaunchVelocity(Vector2 origin, Vector2 target, float gravityScale)
    {
        Vector2 straight = (target - origin).normalized * launchSpeed;

        float G = -Physics2D.gravity.y * gravityScale;
        float dx = Mathf.Abs(target.x - origin.x);
        float dy = target.y - origin.y;
        float v2 = launchSpeed * launchSpeed;
        float disc = v2 * v2 - G * (G * dx * dx + 2f * dy * v2);

        if (G <= 0f || dx < 0.01f || disc < 0f) return straight;

        float angle = Mathf.Atan((v2 - Mathf.Sqrt(disc)) / (G * dx));
        return new Vector2(
            Mathf.Sign(target.x - origin.x) * Mathf.Cos(angle),
            Mathf.Sin(angle)) * launchSpeed;
    }

    private void UseAbility()
    {
        isAbilityOn = true;
        abilityCharge -= pointsToActivate;

        switch (currentAbility)
        {
            case 0: player2CurrentHP -= effectValue; break;
            case 1: player1CurrentHP = Mathf.Min(player1MaxHP, player1CurrentHP + effectValue); break;
            case 2: maxBalls += 1; break;
            case 3:
                for (int i = 0; i < refreshNumber; i++)
                {
                    Vector3 spawnArea = new Vector3(
                        Random.Range(refreshArea.bounds.min.x, refreshArea.bounds.max.x),
                        Random.Range(refreshArea.bounds.min.y, refreshArea.bounds.max.y),
                        15);

                    var instance = Instantiate(pegPrefab, spawnArea, Quaternion.identity);
                    var fb = instance.GetComponent<FeedbackBalls>();
                    if (fb != null) fb.HoldForSpawnPop();
                }
                break;
            default: break;
        }

        variablesForUI = new int[] { 2, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge };
        _turnUpdateUI.Raise(this, variablesForUI);

        Image chosenCutin = PickRandomCutinForCurrentAbility();
        if (chosenCutin != null)
        {
            if (cutinRoutine != null) StopCoroutine(cutinRoutine);
            cutinRoutine = StartCoroutine(PlayAbilityCutinRoutine(chosenCutin));
        }
        else
        {
            isAbilityOn = false;
        }
    }

    private Image PickRandomCutinForCurrentAbility()
    {
        Image[] pool = currentAbility switch
        {
            0 => damageCutinImages,
            1 => healCutinImages,
            2 => cloneCutinImages,
            3 => refreshCutinImages,
            _ => null
        };

        if (pool == null || pool.Length == 0) return null;

        List<Image> valid = new List<Image>();
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null) valid.Add(pool[i]);
        }

        if (valid.Count == 0) return null;
        return valid[Random.Range(0, valid.Count)];
    }

    private void HideAllCutins()
    {
        SetArrayActive(damageCutinImages, false);
        SetArrayActive(healCutinImages, false);
        SetArrayActive(cloneCutinImages, false);
        SetArrayActive(refreshCutinImages, false);
    }

    private void SetArrayActive(Image[] images, bool active)
    {
        if (images == null) return;
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null) images[i].gameObject.SetActive(active);
        }
    }

    private IEnumerator PlayAbilityCutinRoutine(Image cutinImage)
    {
        HideAllCutins();
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        RectTransform rect = cutinImage.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 enterOffset = new Vector2(-250f, 0f);

        cutinImage.gameObject.SetActive(true);
        rect.anchoredPosition = startPos + enterOffset;

        // Animación de entrada (sin tocar tamaño ni escala)
        float t = 0f;
        float inDur = Mathf.Max(0.01f, abilityInDuration);
        while (t < inDur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / inDur);
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            rect.anchoredPosition = Vector2.LerpUnclamped(startPos + enterOffset, startPos, ease);
            yield return null;
        }

        rect.anchoredPosition = startPos;

        // Espera mientras se muestra el cut-in
        if (abilityWaitTime > 0f)
        {
            yield return new WaitForSecondsRealtime(abilityWaitTime);
        }

        // Animación de salida (sin tocar tamaño ni escala)
        t = 0f;
        float outDur = Mathf.Max(0.01f, abilityInDuration);
        Vector2 exitOffset = new Vector2(250f, 0f);
        while (t < outDur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / outDur);
            float ease = p * p;
            rect.anchoredPosition = Vector2.LerpUnclamped(startPos, startPos + exitOffset, ease);
            yield return null;
        }

        cutinImage.gameObject.SetActive(false);
        rect.anchoredPosition = startPos;

        if (abilityWaitToUnfreeze > 0f)
        {
            yield return new WaitForSecondsRealtime(abilityWaitToUnfreeze);
        }

        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
        isAbilityOn = false;
        cutinRoutine = null;
    }

    public void SetDamageAbility()
    {
        currentAbility = 0;
    }
    public void SetHealAbility()
    {
        currentAbility = 1;
    }
    public void SetCloneAbility()
    {
        currentAbility = 2;
    }
    public void SetRefreshAbility()
    {
        currentAbility = 3;
    }
    public void ToggleAbility()
    {
        currentAbility += 1;
        if (currentAbility == 4)
        {
            currentAbility = 0;
        }
    }
}