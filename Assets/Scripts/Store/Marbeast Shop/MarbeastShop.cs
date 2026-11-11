using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarbeastShop : MonoBehaviour
{
    [Header("Card references")]
    [SerializeField] private MarbeastItem marbeast;
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceOrStatusText;
    [SerializeField] private Button actionButton;

    [SerializeField] private bool bakugan;
    [SerializeField] private PersistentBullshit persistentBullshit;

    void Start()
    {
        if (previewImage != null && marbeast.icon != null) previewImage.sprite = marbeast.icon;
        if (nameText != null) nameText.text = marbeast.displayName;

        actionButton.onClick.AddListener(OnClickAction);
        UpdateUI();
    }

    void OnClickAction()
    {
        if (MarbeastSave.IsUnlocked(marbeast)) return;

        if (!MarbeastSave.TryBuy(marbeast))
        {
            Debug.Log("Not enough coins.");
            return;
        }

        foreach (var shop in FindObjectsByType<MarbeastShop>(FindObjectsSortMode.None))
            shop.UpdateUI();

        if (bakugan)
        {
            persistentBullshit.bakugan1 = true;
        }
        else
        {
            persistentBullshit.bakugan2 = true;
        }
    }

    public void UpdateUI()
    {
        if (MarbeastSave.IsUnlocked(marbeast))
        {
            priceOrStatusText.text = "Bought";
            actionButton.interactable = false;
        }
        else
        {
            priceOrStatusText.text = "$" + marbeast.price;
            actionButton.interactable = true;
        }
    }

    void OnEnable()
    {
        MarbeastSave.OnReset += HandleReset;
        if (marbeast != null && actionButton != null) UpdateUI();
    }

    void OnDisable()
    {
        MarbeastSave.OnReset -= HandleReset;
    }

    void HandleReset()
    {
        UpdateUI();
    }
}