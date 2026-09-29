using UnityEngine;

public class ShopClickAudioTrigger : MonoBehaviour
{
    private int clickCount = 0;

    void OnMouseDown()
    {
        if (ShopAudioAndDialogueManager.Instance == null) return;

        clickCount++;

        if (clickCount == 1)
        {
            ShopAudioAndDialogueManager.Instance.SetApproachState();
        }
        else if (clickCount >= 2)
        {
            ShopAudioAndDialogueManager.Instance.EnterShopState();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            clickCount = 0;
            if (ShopAudioAndDialogueManager.Instance != null)
            {
                ShopAudioAndDialogueManager.Instance.SetStreetState();
            }
        }
    }
}