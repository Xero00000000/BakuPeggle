using UnityEngine;

public class NpcTriggerBridge : MonoBehaviour
{
    [SerializeField] private NPCInteraction npcInteraction;

    public void TriggerDialogue()
    {
        if (npcInteraction != null)
        {
            npcInteraction.StartDialogue();
        }
    }
}