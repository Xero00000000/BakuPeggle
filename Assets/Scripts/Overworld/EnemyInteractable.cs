using System.Collections;
using UnityEngine;

public class EnemyInteractable : MonoBehaviour
{
    [Header("Configuración")]
    public string enemyName = "Sombra";
    [TextArea(2, 4)]
    public string[] dialogueLines = new string[]
    {
        "¡Te encontré!",
        "¡No vas a escapar de esta!"
    };

    [Header("Referencias")]
    public Transform playerTransform;
    public GameObject alertIconUI;
    public Vector3 iconWorldOffset = new Vector3(0, 2f, 0);

    [Header("Detección")]
    public float triggerDistance = 3.5f; // Aumentamos un poco el rango

    private bool hasTriggered = false;

    void Start()
    {
        if (playerTransform == null)
        {
            PlayerScrollerMovement mov = FindObjectOfType<PlayerScrollerMovement>();
            if (mov != null) playerTransform = mov.transform;
        }

        if (playerTransform == null)
        {
            Debug.LogError("[EnemyInteractable] No se encontró al Player. Arrástralo a la casilla en el Inspector.");
        }
    }

    void Update()
    {
        if (hasTriggered || playerTransform == null) return;

        // Medimos solo en el eje horizontal X para evitar problemas de altura/profundidad
        float distanceX = Mathf.Abs(transform.position.x - playerTransform.position.x);

        if (distanceX <= triggerDistance)
        {
            hasTriggered = true;
            Debug.Log("<color=green>[EnemyInteractable] ¡Jugador detectado! Iniciando secuencia...</color>");
            StartCoroutine(TriggerEncounterRoutine());
        }
    }

    private IEnumerator TriggerEncounterRoutine()
    {
        // 1. Congelar Player
        PlayerScrollerMovement movement = playerTransform.GetComponent<PlayerScrollerMovement>();
        if (movement != null)
        {
            movement.SetMovementState(false);
            Debug.Log("[1/3] Movimiento de jugador congelado.");
        }

        // 2. Mostrar Ícono
        if (alertIconUI != null)
        {
            alertIconUI.SetActive(true);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + iconWorldOffset);
            alertIconUI.transform.position = screenPos;
            Debug.Log("[2/3] Ícono de alerta mostrado.");
        }

        yield return new WaitForSeconds(0.4f);

        if (alertIconUI != null) alertIconUI.SetActive(false);

        // 3. Diálogo
        if (DialogueUI.Instance != null)
        {
            Debug.Log("[3/3] Abriendo diálogo...");
            DialogueUI.Instance.StartDialogue(enemyName, dialogueLines, () =>
            {
                Debug.Log("Diálogo terminado. Iniciando destello...");
                if (CombatTransitionManager.Instance != null)
                {
                    CombatTransitionManager.Instance.TriggerTransition(
                        onPeakAction: () => Debug.Log("<color=red>--- ESTADO DE COMBATE ACTIVO ---</color>"),
                        onComplete: () => Debug.Log("<color=cyan>--- Transición terminada ---</color>")
                    );
                }
            });
        }
        else
        {
            Debug.LogError("[ERROR] DialogueUI.Instance es NULL. Verificá que el script DialogueUI esté en el Canvas.");
        }
    }
}