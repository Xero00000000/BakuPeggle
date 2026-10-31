using UnityEngine;
using UnityEngine.UI;

public class ShopResetButton : MonoBehaviour
{
    [Header("Every marbeast in the shop")]
    [SerializeField] private MarbeastItem[] allMarbeasts;

    [SerializeField] private Button resetButton;
    [SerializeField] private bool resetCoins = true;

    void Start()
    {
        if (resetButton == null) resetButton = GetComponent<Button>();
        resetButton.onClick.AddListener(DoReset);
    }

    public void DoReset()
    {
        MarbeastSave.ResetPurchases(allMarbeasts, resetCoins);
    }
}