using UnityEngine;
using TMPro;

public class ShopCoinDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText; 

    private const string COINS_KEY = "PlayerCoins";

    void Start()
    {
        UpdateCoinDisplay();
    }

    void Update()
    {
        UpdateCoinDisplay();
    }

    void UpdateCoinDisplay()
    {
        int currentCoins = PlayerPrefs.GetInt(COINS_KEY, 500);

        if (coinText != null)
        {
            coinText.text = currentCoins.ToString();
        }
    }
}