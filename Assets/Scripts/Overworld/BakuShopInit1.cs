using TMPro;
using UnityEngine;

public class BakuShopInit1 : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    void Start()
    {
        if (ShopAudioAndDialogueManager.Instance != null)
        {
            ShopAudioAndDialogueManager.Instance.dialoguePanel = dialoguePanel;
            ShopAudioAndDialogueManager.Instance.dialogueText = dialogueText;
            ShopAudioAndDialogueManager.Instance.EnterShopState();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (ShopAudioAndDialogueManager.Instance != null)
            {
                ShopAudioAndDialogueManager.Instance.NextDialogue();
            }
        }
    }
}