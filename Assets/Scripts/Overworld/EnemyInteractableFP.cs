using System.Collections;
using UnityEngine;
using PrimeTween;

public class EnemyInteractableFP : MonoBehaviour
{
    [Header("Datos")]
    public string enemyName = "Sombra";
    [TextArea(2, 4)]
    public string[] dialogueLines = new string[]
    {
        "¡No deberías estar aquí!",
        "¡Prepárate para pelear!"
    };

    [Header("Detección")]
    [Tooltip("Distancia en metros a la que te detecta")]
    public float triggerDistance = 5.0f;

    [Tooltip("Tolerancia del cono de visión en grados (más alto = más permisivo)")]
    public float visionAngle = 110f;

    [Header("Alerta Visual")]
    public Transform alertIconTransform;

    private Transform playerCameraTransform;
    private FirstPersonController playerController;
    private bool hasTriggered = false;

    void Start()
    {
        // Buscamos la cámara del jugador para saber exactamente hacia dónde estás mirando
        Camera cam = Camera.main;
        if (cam != null)
        {
            playerCameraTransform = cam.transform;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<FirstPersonController>();
            if (playerCameraTransform == null) playerCameraTransform = playerObj.transform;
        }

        if (alertIconTransform != null)
            alertIconTransform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (hasTriggered || playerCameraTransform == null) return;

        // Distancia real entre la cámara y el centro del enemigo
        Vector3 enemyCenter = transform.position + Vector3.up * 1.0f; // apuntamos al centro/pecho
        float distance = Vector3.Distance(playerCameraTransform.position, enemyCenter);

        if (distance <= triggerDistance)
        {
            // Vector desde los ojos del jugador hacia el enemigo
            Vector3 dirToEnemy = (enemyCenter - playerCameraTransform.position).normalized;

            // Evaluamos el ángulo entre la mirada de la cámara y el enemigo
            float angle = Vector3.Angle(playerCameraTransform.forward, dirToEnemy);

            // Si el enemigo está dentro del cono visible de la pantalla
            if (angle <= visionAngle * 0.5f)
            {
                hasTriggered = true;
                StartCoroutine(TriggerSequenceRoutine());
            }
        }
    }

    private IEnumerator TriggerSequenceRoutine()
    {
        if (playerController != null)
            playerController.SetControlState(false);

        // Pop-in del '!'
        if (alertIconTransform != null)
        {
            Tween.Scale(alertIconTransform, endValue: Vector3.one, duration: 0.3f, ease: Ease.OutBack);
        }

        yield return new WaitForSeconds(0.5f);

        if (alertIconTransform != null)
        {
            Tween.Scale(alertIconTransform, endValue: Vector3.zero, duration: 0.15f, ease: Ease.InQuad);
        }

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.StartDialogue(enemyName, dialogueLines, () =>
            {
                if (CombatTransitionManager.Instance != null)
                {
                    CombatTransitionManager.Instance.TriggerTransition(
                        onPeakAction: () => Debug.Log("<color=red>--- CARGAR ESCENA DE COMBATE ---</color>"),
                        onComplete: () => Debug.Log("<color=green>--- Transición finalizada ---</color>")
                    );
                }
            });
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}