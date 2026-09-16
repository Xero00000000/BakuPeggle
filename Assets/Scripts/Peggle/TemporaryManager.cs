using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; 

public class TemporaryManager : MonoBehaviour
{
    private bool player1Shot = false;
    private bool player2Shot = false;

    public bool ball1Destroyed = false;
    public bool ball2Destroyed = false;

    private bool isPlayer2Turn;

    [Header("Salud de Jugadores")]
    [SerializeField] private int player1MaxHP = 100;
    [SerializeField] private int player2MaxHP = 100;
    [SerializeField] private int player1CurrentHP;
    [SerializeField] private int player2CurrentHP;

    [Header("Event Channels")]
    [SerializeField] private ShotEventChannel _shoot;
    [SerializeField] private VoidEventChannel _newTurn;

    [Header("Configuración del Enemigo")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private float force;
    [SerializeField] private float spawnPointMinRotation;
    [SerializeField] private float spawnPointMaxRotation;

    [Header("UI Canvas Final de Juego")]
    [Tooltip("Canvas que se activará cuando el Jugador 1 gane (Player 2 HP <= 0).")]
    [SerializeField] private GameObject winCanvas;

    [Tooltip("Canvas que se activará cuando el Jugador 1 pierda (Player 1 HP <= 0).")]
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

    public void Start()
    {
        player1CurrentHP = player1MaxHP;
        player2CurrentHP = player2MaxHP;

        if (winCanvas != null) winCanvas.SetActive(false);
        if (loseCanvas != null) loseCanvas.SetActive(false);

        if (player1PointsCanvas != null) player1PointsCanvas.SetActive(false);
        if (player2PointsCanvas != null) player2PointsCanvas.SetActive(false);

        ResetTurn();
    }

    public void Update()
    {
        if (isGameOver) return;

        if (player1Shot && player2Shot && ball1Destroyed && ball2Destroyed)
        {
            ResetTurn();
        }
    }

    private void ResetTurn()
    {
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

    private void HandleGameOver()
    {
        isGameOver = true;

        if (player1PointsCanvas != null) player1PointsCanvas.SetActive(false);
        if (player2PointsCanvas != null) player2PointsCanvas.SetActive(false);

        if (player2CurrentHP <= 0)
        {
            if (winCanvas != null) winCanvas.SetActive(true);
        }
        else if (player1CurrentHP <= 0)
        {
            if (loseCanvas != null) loseCanvas.SetActive(true);
        }
    }

    public void WasShot(Component sender, int which)
    {
        if (isGameOver) return;

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
        switch (type)
        {
            case 0:
                points = 15;
                break;
            case 1:
                points = 30;
                break;
            case 2:
                points = 60;
                break;
        }

        if (player)
        {
            player1CurrentHP -= points;
            ShowPlayerPoints(1, points);
        }
        else
        {
            player2CurrentHP -= points;
            ShowPlayerPoints(2, points);
        }

        if (player1CurrentHP <= 0 || player2CurrentHP <= 0)
        {
            HandleGameOver();
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
        if (isGameOver) return;

        float randomZ = Random.Range(spawnPointMinRotation, spawnPointMaxRotation);
        spawnPoint.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);
        var instance = Instantiate(ballPrefab, spawnPoint.transform.position, Quaternion.identity);
        instance.GetComponent<Rigidbody2D>().AddForce(spawnPoint.transform.up * force);
        player2Shot = true;
    }
}