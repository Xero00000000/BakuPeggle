using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
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
    [Tooltip("True = Habilidad de Daño | False = Habilidad de Curación")]
    [SerializeField] private int currentAbility;

    [Header("Configuración de Prefabs de Bolas")]
    [SerializeField] private GameObject damageBallPrefab; // Usado cuando la habilidad de daño está activa
    [SerializeField] private GameObject healBallPrefab;   // Usado cuando la habilidad de curación está activa
    [SerializeField] private GameObject cloneBallPrefab;
    [SerializeField] private GameObject bombBallPrefab;

    [Header("Configuración de Habilidades")]
    [SerializeField] private int damageCost = 150;
    [SerializeField] private int damageEffectValue = 100;

    [SerializeField] private int healCost = 300;
    [SerializeField] private int healEffectValue = 150;

    [SerializeField] private int multiplyCost = 150;

    [SerializeField] private int refreshCost = 300;
    //[SerializeField] private GameObject refreshArea;
    [SerializeField] private BoxCollider refreshArea;
    [SerializeField] private GameObject pegPrefab;
    [SerializeField] private int refreshNumber;
 
    // Propiedades dinámicas basadas en la habilidad seleccionada
    private int pointsToActivate => currentAbility switch
    {
        0 => damageCost,
        1 => healCost,
        2 => multiplyCost,
        3 => refreshCost,
        _ => 0
    };
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
    [SerializeField] private float spawnPointMinRotation;
    [SerializeField] private float spawnPointMaxRotation;


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

    [SerializeField] private int[] variablesForUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        player1CurrentHP = player1MaxHP;
        player2CurrentHP = player2MaxHP;
        abilityCharge = 0;
        maxBalls = 1;
        currentBallsDestroyed = 0;


        variablesForUI = new int[] {0, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge};
        _turnUpdateUI.Raise(this, variablesForUI);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver || isEndingTurn) return;

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
            case 2: isAbilityOn = false; ; break;

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

        //Continue(this, 1);
        _turnUpdateUI.Raise(this, variablesForUI);
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        variablesForUI = new int[] { 4, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge };
        _turnUpdateUI.Raise(this, variablesForUI);
    }

    public void WasShot(Component sender, int which)
    {
        if (isGameOver || isAbilityOn || ignoreShootThisFrame) return;

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
            case 0: points = 15; charge += 15; break;
            case 1: points = 30; charge += 30; break;
            case 2: points = 60; charge += 15; break;
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

        float randomZ = Random.Range(spawnPointMinRotation, spawnPointMaxRotation);
        spawnPoint.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);

        // Selecciona el prefab de bola según la habilidad activa
        GameObject prefabToSpawn = currentAbility switch
        {
            0 => damageBallPrefab,
            1 => healBallPrefab,
            2 => cloneBallPrefab,
            3 => bombBallPrefab,
            _ => null
        };

        if (prefabToSpawn != null)
        {
            var instance = Instantiate(prefabToSpawn, spawnPoint.transform.position, Quaternion.identity);
            instance.GetComponent<Rigidbody2D>().AddForce(spawnPoint.transform.up * force);
        }

        player2Shot = true;
    }

    private void UseAbility()
    {
        isAbilityOn = true;
        abilityCharge -= pointsToActivate;

        // Si la habilidad de daño está activa: Quita 100 de vida al enemigo (Player 2)
        // Si está activa la de curación: Cura 150 de vida al Jugador 1
        switch (currentAbility)
        {
            case 0: player2CurrentHP -= effectValue; break;
            case 1: player1CurrentHP = Mathf.Min(player1MaxHP, player1CurrentHP + effectValue); break;
            case 2: maxBalls += 1; break;
            case 3:
                for (int i = 0; i < refreshNumber; i++)
                {
                    Vector3 spawnArea = new Vector3(Random.Range(refreshArea.bounds.min.x, refreshArea.bounds.max.x), Random.Range(refreshArea.bounds.min.y, refreshArea.bounds.max.y), 15);
                    var instance = Instantiate(pegPrefab, spawnArea, Quaternion.identity);
                }
                break;
            default: break;
        };

        variablesForUI = new int[] { 2, player1CurrentHP, player1MaxHP, player2CurrentHP, player2MaxHP, player1DamageToTake, player2DamageToTake, abilityCharge };
        _turnUpdateUI.Raise(this, variablesForUI);
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
    public void SetBombAbility()
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
