using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TemporaryManager : MonoBehaviour
{
    private bool player1Shot = false;
    private bool player2Shot = false;

    public bool ball1Destroyed = false;
    public bool ball2Destroyed = false;

    [SerializeField] private int player1MaxHP;
    [SerializeField] private int player2MaxHP;
    [SerializeField] private int player1CurrentHP;
    [SerializeField] private int player2CurrentHP;

    [SerializeField] private ShotEventChannel _shoot;
    [SerializeField] private VoidEventChannel _newTurn;

    [SerializeField] private GameObject ballPrefab;

    public void Start()
    {
        player1CurrentHP = player1MaxHP;
        player2CurrentHP = player2MaxHP;
        ResetTurn();
    }

    public void Update()
    {
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

        }
        else
        {
            _newTurn.Raise(this);
        }
    }

    public void WasShot(Component sender, int which)
    {
        if (which == 1)
        {
            player1Shot = true;
        }
    }

    public void AddPoints(Component sender, bool player, int type)
    {
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

        if (player == true)
        {
            player1CurrentHP = player1CurrentHP - points;
        }
        else
        {
            player2CurrentHP = player2CurrentHP - points;
        }
    }
}
