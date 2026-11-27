using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ShopBuyManager : MonoBehaviour
{
    [Header("UI General")]
    [SerializeField] private TMP_Text coinText;              // Texto de monedas

    [Header("Configuración de Fondos y Visuales")]
    [SerializeField] private Image backgroundDisplayImage;     // La Image del fondo (si queda vacía, la busca sola)
    [SerializeField] private List<Sprite> wallpaperSprites;    // Las imágenes (sprites) de los fondos en orden
    [SerializeField] private List<int> wallpaperPrices;        // Precios de cada fondo
    [SerializeField] private List<Button> shopButtons;        // Botones de la tienda
    [SerializeField] private List<TMP_Text> buttonTexts;      // Textos de los botones

    // Las monedas ahora viven en MarbeastSave.Coins (una sola billetera para todas las tiendas)
    public static int selectedWallpaperIndex = -1;

    private static Dictionary<int, bool> sessionUnlockedWallpapers = new Dictionary<int, bool>();

    void Awake()
    {
        // Si no asignaste la imagen en el Inspector, la busca automáticamente
        if (backgroundDisplayImage == null)
        {
            backgroundDisplayImage = GetComponent<Image>();
            if (backgroundDisplayImage == null)
            {
                backgroundDisplayImage = FindObjectOfType<Image>();
            }
        }
    }

    void Start()
    {
        ActualizarVista();
        ActualizarFondoVisual();
        ActualizarEstadoBotones();
    }

    void Update()
    {
        ActualizarVista();
    }

    void ActualizarVista()
    {
        if (coinText != null)
        {
            coinText.text = MarbeastSave.Coins.ToString();
        }
    }

    // Cambia la imagen visual del fondo según el índice seleccionado
    void ActualizarFondoVisual()
    {
        if (backgroundDisplayImage == null)
        {
            GameObject bgObj = GameObject.FindWithTag("BackgroundShop"); // Opcional si le ponés un Tag
            if (bgObj != null) backgroundDisplayImage = bgObj.GetComponent<Image>();
        }

        if (backgroundDisplayImage != null && wallpaperSprites != null && selectedWallpaperIndex >= 0 && wallpaperSprites.Count > selectedWallpaperIndex)
        {
            backgroundDisplayImage.sprite = wallpaperSprites[selectedWallpaperIndex];

            Color c = backgroundDisplayImage.color;
            c.a = 1f;
            backgroundDisplayImage.color = c;
        }
    }

    void ActualizarEstadoBotones()
    {
        int coins = MarbeastSave.Coins;

        for (int i = 0; i < wallpaperPrices.Count; i++)
        {
            bool isUnlocked = sessionUnlockedWallpapers.ContainsKey(i) && sessionUnlockedWallpapers[i];
            int price = wallpaperPrices[i];

            // 1. Actualizar Textos
            if (i < buttonTexts.Count && buttonTexts[i] != null)
            {
                if (isUnlocked)
                {
                    buttonTexts[i].text = (selectedWallpaperIndex == i) ? "Seleccionado" : "Seleccionar";
                }
                else
                {
                    buttonTexts[i].text = "Comprar (" + price + ")";
                }
            }

            // 2. Activar o Desactivar el Botón
            if (i < shopButtons.Count && shopButtons[i] != null)
            {
                if (isUnlocked)
                {
                    shopButtons[i].interactable = (selectedWallpaperIndex != i);
                }
                else
                {
                    shopButtons[i].interactable = (coins >= price);
                }
            }
        }
    }

    public void OnItemButtonClicked(int index)
    {
        if (index < 0 || index >= wallpaperPrices.Count) return;

        bool isUnlocked = sessionUnlockedWallpapers.ContainsKey(index) && sessionUnlockedWallpapers[index];

        if (isUnlocked)
        {
            // Seleccionar fondo ya comprado
            selectedWallpaperIndex = index;
            ActualizarFondoVisual();
            ActualizarEstadoBotones();
        }
        else
        {
            // Intentar comprar
            int price = wallpaperPrices[index];

            if (MarbeastSave.Coins >= price)
            {
                MarbeastSave.Coins -= price;
                PlayerPrefs.Save();

                sessionUnlockedWallpapers[index] = true;
                selectedWallpaperIndex = index;

                ActualizarVista();
                ActualizarFondoVisual();
                ActualizarEstadoBotones();
            }
        }
    }
}