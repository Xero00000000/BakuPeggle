using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ShopController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinText;          

    [Header("Configuración de la Tienda")]
    [SerializeField] private List<int> wallpaperPrices;    
    [SerializeField] private List<bool> stock;            

    private const string COINS_KEY = "PlayerCoins";
    private int currentCoins;

    void Start()
    {
        ActualizarVistaMonedas();
    }

    void Update()
    {
        ActualizarVistaMonedas();
    }

    void ActualizarVistaMonedas()
    {
        currentCoins = PlayerPrefs.GetInt(COINS_KEY, 50); 

        if (coinText != null)
        {
            coinText.text = currentCoins.ToString();
        }
    }

    public bool BuyWallpaper(int index)
    {
        if (index < 0 || index >= wallpaperPrices.Count) return false;

        currentCoins = PlayerPrefs.GetInt(COINS_KEY, 50);
        int price = wallpaperPrices[index];
        string stockKey = "Wallpaper_" + index;
        bool isUnlocked = PlayerPrefs.GetInt(stockKey, 0) == 1;

        if (!isUnlocked && currentCoins >= price)
        {
            currentCoins -= price;
            PlayerPrefs.SetInt(COINS_KEY, currentCoins);
            PlayerPrefs.SetInt(stockKey, 1); 
            PlayerPrefs.Save();

            ActualizarVistaMonedas();
            Debug.Log("¡Fondo comprado con éxito! Monedas restantes: " + currentCoins);
            return true;
        }

        Debug.Log("No se pudo comprar (Faltan monedas o ya está desbloqueado).");
        return false;
    }
}