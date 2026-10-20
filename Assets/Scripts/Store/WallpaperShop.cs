using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WallpaperShop : MonoBehaviour
{
    [Header("Referencias de esta tarjeta")]
    [SerializeField] private WallpaperItem wallpaperData;    
    [SerializeField] private Image previewImage;             
    [SerializeField] private TMP_Text priceOrStatusText;      
    [SerializeField] private Button actionButton;             

    [Header("Fondo Global de la Escena")]
    [SerializeField] private Image backgroundTarget;         

    private const string PREF_PREFIX = "Wallpaper_";
    private const string PREF_EQUIPPED = "EquippedWallpaper";

    void Start()
    {
        if (previewImage != null && wallpaperData != null && wallpaperData.wallpaperSprite != null)
        {
            previewImage.sprite = wallpaperData.wallpaperSprite;
        }

        UpdateUI();
        actionButton.onClick.AddListener(OnClickAction);
    }

    public void RefreshUI()
    {
        UpdateUI();
    }

    void OnClickAction()
    {
        bool isUnlocked = PlayerPrefs.GetInt(PREF_PREFIX + wallpaperData.wallpaperId, 0) == 1;

        if (!isUnlocked)
        {
            int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 500); 

            if (currentCoins >= wallpaperData.price)
            {
                currentCoins -= wallpaperData.price;
                PlayerPrefs.SetInt("PlayerCoins", currentCoins);

                PlayerPrefs.SetInt(PREF_PREFIX + wallpaperData.wallpaperId, 1);
                EquipWallpaper();
            }
            else
            {
                Debug.Log("No te alcanzan las monedas.");
            }
        }
        else
        {
            EquipWallpaper();
        }

        WallpaperShop[] allShops = FindObjectsOfType<WallpaperShop>();
        foreach (WallpaperShop shop in allShops)
        {
            shop.RefreshUI();
        }
    }

    void EquipWallpaper()
    {
        PlayerPrefs.SetString(PREF_EQUIPPED, wallpaperData.wallpaperId);

        if (backgroundTarget != null && wallpaperData.wallpaperSprite != null)
        {
            backgroundTarget.sprite = wallpaperData.wallpaperSprite;
        }
    }

    void UpdateUI()
    {
        bool isUnlocked = PlayerPrefs.GetInt(PREF_PREFIX + wallpaperData.wallpaperId, 0) == 1;
        string equippedId = PlayerPrefs.GetString(PREF_EQUIPPED, "");

        if (!isUnlocked)
        {
            priceOrStatusText.text = "$" + wallpaperData.price;
        }
        else if (equippedId == wallpaperData.wallpaperId)
        {
            priceOrStatusText.text = "Equipado";
        }
        else
        {
            priceOrStatusText.text = "Seleccionar";
        }
    }
}