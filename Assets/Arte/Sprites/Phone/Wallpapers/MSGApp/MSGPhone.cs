using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MSGPhone : MonoBehaviour
{
    public enum StepType
    {
        SingleObject,
        ChoiceBreak
    }

    [Serializable]
    public class SequenceStep
    {
        public string label;
        public StepType stepType = StepType.SingleObject;

        [Tooltip("Usado si el tipo es SingleObject")]
        public GameObject targetObject;

        [Tooltip("Segundos de espera antes de pasar al siguiente paso")]
        public float delayAfter = 2f;

        [Tooltip("Usado si el tipo es ChoiceBreak: se activan todos a la vez")]
        public GameObject[] choiceGroupObjects;

        [Tooltip("Si es true, desactiva los objetos del paso anterior al avanzar")]
        public bool deactivatePrevious = true;
    }

    [Header("Secuencia de Mensajes")]
    public bool playOnEnable = true;
    public bool useUnscaledTime = true;
    public List<SequenceStep> steps = new List<SequenceStep>();

    [Header("Cambio de Escena al Finalizar")]
    public bool loadSceneOnFinish = false;

    // Arrastra aquí el archivo de escena (.unity) desde la carpeta Project
    public UnityEngine.Object nextSceneAsset;

    [Tooltip("Opcional: o escribe el nombre exacto de la escena si no usas el campo de arriba")]
    public string nextSceneName;

    [Tooltip("Espera extra antes de cambiar de escena al terminar")]
    public float endDelay = 1f;

    private Coroutine sequenceCoroutine;
    private GameObject[] lastActiveGroup;
    private GameObject lastActiveSingle;
    private bool isWaitingAtBreak = false;

    void OnValidate()
    {
        if (nextSceneAsset != null)
        {
            nextSceneName = nextSceneAsset.name;
        }
    }

    void OnEnable()
    {
        if (playOnEnable)
        {
            StartSequence();
        }
    }

    void OnDisable()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        isWaitingAtBreak = false;
    }

    public void StartSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        isWaitingAtBreak = false;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        if (seconds <= 0f) yield break;

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }

    private IEnumerator RunSequence()
    {
        // Apaga todos los objetos al inicio para empezar limpio
        foreach (var step in steps)
        {
            if (step.targetObject != null) step.targetObject.SetActive(false);
            if (step.choiceGroupObjects != null)
            {
                foreach (var obj in step.choiceGroupObjects)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }
        }

        lastActiveSingle = null;
        lastActiveGroup = null;

        for (int i = 0; i < steps.Count; i++)
        {
            SequenceStep current = steps[i];

            if (current.stepType == StepType.SingleObject)
            {
                if (current.deactivatePrevious)
                {
                    DeactivatePrevious();
                }

                if (current.targetObject != null)
                {
                    current.targetObject.SetActive(true);
                    lastActiveSingle = current.targetObject;
                    lastActiveGroup = null;
                }

                yield return WaitSeconds(current.delayAfter);
            }
            else if (current.stepType == StepType.ChoiceBreak)
            {
                if (current.deactivatePrevious)
                {
                    DeactivatePrevious();
                }

                if (current.choiceGroupObjects != null)
                {
                    foreach (var choiceObj in current.choiceGroupObjects)
                    {
                        if (choiceObj != null)
                        {
                            choiceObj.SetActive(true);
                        }
                    }
                    lastActiveGroup = current.choiceGroupObjects;
                    lastActiveSingle = null;
                }

                // Activa la pausa y espera hasta que un botón llame a ContinueAfterBreak()
                isWaitingAtBreak = true;
                yield return new WaitUntil(() => !isWaitingAtBreak);
            }
        }

        if (endDelay > 0f)
        {
            yield return WaitSeconds(endDelay);
        }

        FinishSequenceAndLoadScene();
    }

    // Asignar al OnClick de los botones de opción en un ChoiceBreak
    public void ContinueAfterBreak()
    {
        isWaitingAtBreak = false;
    }

    public void FinishDialogAndLoadScene()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        isWaitingAtBreak = false;
        FinishSequenceAndLoadScene();
    }

    private void FinishSequenceAndLoadScene()
    {
        if (!loadSceneOnFinish) return;

        string sceneToLoad = nextSceneAsset != null ? nextSceneAsset.name : nextSceneName;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("MSGPhone: No se asignó ninguna escena en 'Next Scene Asset' o 'Next Scene Name'.");
        }
    }

    private void DeactivatePrevious()
    {
        if (lastActiveSingle != null)
        {
            lastActiveSingle.SetActive(false);
            lastActiveSingle = null;
        }

        if (lastActiveGroup != null)
        {
            foreach (var obj in lastActiveGroup)
            {
                if (obj != null) obj.SetActive(false);
            }
            lastActiveGroup = null;
        }
    }
}