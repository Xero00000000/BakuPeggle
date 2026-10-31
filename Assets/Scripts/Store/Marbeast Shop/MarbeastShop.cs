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

    void Start()
    {
        if (previewImage != null && marbeast.icon != null) previewImage.sprite = marbeast.icon;
        if (nameText != null) nameText.text = marbeast.displayName;

        // Make sure the default marbeast is equipped on first run
        if (marbeast.unlockedByDefault && !PlayerPrefs.HasKey(MarbeastSave.EQUIPPED_INDEX_KEY))
            MarbeastSave.Equip(marbeast);

        actionButton.onClick.AddListener(OnClickAction);
        UpdateUI();
    }

    void OnClickAction()
    {
        if (!MarbeastSave.IsUnlocked(marbeast))
        {
            if (!MarbeastSave.TryBuy(marbeast))
            {
                Debug.Log("Not enough coins.");
                return;
            }
        }

        MarbeastSave.Equip(marbeast);

        foreach (var shop in FindObjectsByType<MarbeastShop>(FindObjectsSortMode.None))
            shop.UpdateUI();
    }

    public void UpdateUI()
    {
        if (!MarbeastSave.IsUnlocked(marbeast))
        {
            priceOrStatusText.text = "$" + marbeast.price;
            actionButton.interactable = true;
        }
        else if (MarbeastSave.IsEquipped(marbeast))
        {
            priceOrStatusText.text = "Equipped";
            actionButton.interactable = false;
        }
        else
        {
            priceOrStatusText.text = "Select";
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
        if (marbeast.unlockedByDefault)
            MarbeastSave.Equip(marbeast);   // normal marbeast equipped again

        UpdateUI();
    }
}