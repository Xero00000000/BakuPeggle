using UnityEngine;
using TMPro;

public class CoinShop : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TurnManager turnManager;       // drag the TurnManager object here
    [SerializeField] private TMP_Text rewardDisplayText;

    // Only displays the reward. TurnManager.HandleGameOver() is what pays it.
    void OnEnable()
    {
        if (rewardDisplayText != null && turnManager != null)
        {
            rewardDisplayText.text = "+ " + turnManager.WinReward;
        }
    }
}