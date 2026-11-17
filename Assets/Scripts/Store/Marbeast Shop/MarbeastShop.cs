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

    [Header("3D sphere in the shelf")]
    [SerializeField] private MarbeastSphereVanish sphere;

    void Start()
    {
        if (previewImage != null && marbeast.icon != null) previewImage.sprite = marbeast.icon;
        if (nameText != null) nameText.text = marbeast.displayName;

        actionButton.onClick.AddListener(OnClickAction);
        UpdateUI();
        SyncSphere();
    }

    void OnClickAction()
    {
        if (MarbeastSave.IsUnlocked(marbeast)) return;

        if (!MarbeastSave.TryBuy(marbeast))
        {
            Debug.Log("Not enough coins.");
            return;
        }

        if (sphere != null) sphere.Play();

        foreach (var shop in FindObjectsByType<MarbeastShop>(FindObjectsSortMode.None))
            shop.UpdateUI();
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

    // Bought (and not a free default) -> sphere gone. Otherwise -> sphere visible.
    void SyncSphere()
    {
        if (sphere == null || marbeast == null) return;

        bool shouldBeGone = MarbeastSave.IsUnlocked(marbeast) && !marbeast.unlockedByDefault;
        if (shouldBeGone) sphere.SetVanished();
        else sphere.Restore();
    }

    void OnEnable()
    {
        MarbeastSave.OnReset += HandleReset;
        if (marbeast != null && actionButton != null)
        {
            UpdateUI();
            SyncSphere();
        }
    }

    void OnDisable()
    {
        MarbeastSave.OnReset -= HandleReset;
    }

    void HandleReset()
    {
        UpdateUI();
        SyncSphere();
    }
}