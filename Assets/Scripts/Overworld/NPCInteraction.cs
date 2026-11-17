using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI de Diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [TextArea(3, 5)]
    [SerializeField] private string[] dialogueLines;

    [Header("Opciones de Combate")]
    [SerializeField] private GameObject combatChoicePanel;
    [SerializeField] private string combatSceneName = "Peggle 3"; // Escena de destino

    private int currentLineIndex = 0;
    private bool dialogueEnded = false;

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (combatChoicePanel) combatChoicePanel.SetActive(false);
    }

    public void StartDialogue()
    {
        if (dialogueLines.Length == 0) return;

        dialoguePanel.SetActive(true);
        currentLineIndex = 0;
        dialogueEnded = false;
        combatChoicePanel.SetActive(false);

        ShowCurrentLine();
    }

    public void NextLine()
    {
        if (dialogueEnded) return;

        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogueAndAskForCombat();
        }
    }

    private void ShowCurrentLine()
    {
        if (dialogueText != null)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
    }

    private void EndDialogueAndAskForCombat()
    {
        dialogueEnded = true;
        if (dialogueText) dialogueText.gameObject.SetActive(false);
        if (combatChoicePanel) combatChoicePanel.SetActive(true);
    }

    public void AcceptCombat()
    {
        SceneManager.LoadScene(combatSceneName);
    }

    public void DeclineCombat()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (dialogueText) dialogueText.gameObject.SetActive(true);
    }
}