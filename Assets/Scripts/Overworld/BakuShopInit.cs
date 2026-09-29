using UnityEngine;

public class BakuShopInit : MonoBehaviour
{
    void Start()
    {
        if (ShopAudioAndDialogueManager.Instance != null)
        {
            ShopAudioAndDialogueManager.Instance.EnterShopState();
        }
    }
}