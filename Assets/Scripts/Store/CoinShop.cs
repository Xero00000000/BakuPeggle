using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoinShop : MonoBehaviour
{
    [SerializeField] int coins;
    [SerializeField] int coinsToAdd;
    [SerializeField] List<bool> stock;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            coins += coinsToAdd;
        }
    }
}
