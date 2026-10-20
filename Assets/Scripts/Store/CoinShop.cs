using UnityEngine;
using TMPro;

public class CoinShop : MonoBehaviour
{
    [Header("Configuración de Victoria")]
    [SerializeField] private int coinsToAdd = 100;          
    [SerializeField] private TMP_Text rewardDisplayText;    

    void Start()
    {
        if (rewardDisplayText != null)
        {
            rewardDisplayText.text = "+ " + coinsToAdd;
        }

        ShopBuyManager.sessionCoins += coinsToAdd;
        Debug.Log("¡Partida ganada! Monedas actuales en sesión: " + ShopBuyManager.sessionCoins);
    }
}