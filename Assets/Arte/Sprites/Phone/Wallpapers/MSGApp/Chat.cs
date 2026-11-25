using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Chat : MonoBehaviour
{
    [Serializable]
    public class TimedMessage
    {
        public string label;
        public GameObject messageObject;
        [Tooltip("Segundos que espera antes de mostrar el siguiente mensaje o las opciones")]
        public float delayAfter = 1.5f;
    }

    [Serializable]
    public class ChoiceBranch
    {
        public string choiceLabel;
        [Tooltip("Objetos o textos de respuesta que aparecen al elegir esta opción")]
        public GameObject[] responseObjects;
        [Tooltip("Segundos de espera después de mostrar la respuesta")]
        public float delayAfterResponse = 1.5f;
        [Tooltip("Si está marcado, cambia de escena al terminar esta rama")]
        public bool loadScene = false;
        [Tooltip("Arrastra aquí la escena (.unity) desde el Project")]
        public UnityEngine.Object sceneAsset;
        public string sceneName;
    }

    [Header("1. Mensajes Iniciales del NPC")]
    public bool playOnEnable = true;
    public bool useUnscaledTime = true;
    public List<TimedMessage> npcMessages = new List<TimedMessage>();

    [Header("2. Opciones de Respuesta (se activan juntas)")]
    public GameObject[] choiceButtons;

    [Header("3. Ramas Según la Opción Elegida")]
    public ChoiceBranch case1 = new ChoiceBranch { choiceLabel = "Caso 1 (Ej: Mas tarde)" };
    public ChoiceBranch case2 = new ChoiceBranch { choiceLabel = "Caso 2 (Ej: Pasar)" };

    [Header("4. Botón con Espera de 2 Segundos")]
    [Tooltip("Arrastra aquí la escena (.unity) desde el Project para LoadSceneAfterTwoSeconds()")]
    public UnityEngine.Object buttonTargetSceneAsset;
    [Tooltip("Opcional: nombre de escena si no usas el campo de arriba")]
    public string buttonTargetSceneName;
    public float buttonDelaySeconds = 2f;

    private Coroutine flowCoroutine;
    private Coroutine buttonSceneCoroutine;
    private bool choiceSelected = false;
    private int selectedCaseIndex = -1;

    void OnValidate()
    {
        if (case1 != null && case1.sceneAsset != null)
            case1.sceneName = case1.sceneAsset.name;

        if (case2 != null && case2.sceneAsset != null)
            case2.sceneName = case2.sceneAsset.name;

        if (buttonTargetSceneAsset != null)
            buttonTargetSceneName = buttonTargetSceneAsset.name;
    }

    void OnEnable()
    {
        if (playOnEnable)
        {
            StartChat();
        }
    }

    void OnDisable()
    {
        if (flowCoroutine != null)
        {
            StopCoroutine(flowCoroutine);
            flowCoroutine = null;
        }

        if (buttonSceneCoroutine != null)
        {
            StopCoroutine(buttonSceneCoroutine);
            buttonSceneCoroutine = null;
        }
    }

    public void StartChat()
    {
        if (flowCoroutine != null)
            StopCoroutine(flowCoroutine);

        choiceSelected = false;
        selectedCaseIndex = -1;
        flowCoroutine = StartCoroutine(RunChatFlow());
    }

    // Método público para asignar al OnClick() de un Button
    public void LoadSceneAfterTwoSeconds()
    {
        if (buttonSceneCoroutine != null)
            StopCoroutine(buttonSceneCoroutine);

        buttonSceneCoroutine = StartCoroutine(WaitAndLoadButtonScene());
    }

    private IEnumerator WaitAndLoadButtonScene()
    {
        yield return Wait(buttonDelaySeconds);

        string targetScene = buttonTargetSceneAsset != null
            ? buttonTargetSceneAsset.name
            : buttonTargetSceneName;

        // Respaldo por si solo configuraste la escena en Case 2
        if (string.IsNullOrEmpty(targetScene) && case2 != null)
        {
            targetScene = case2.sceneAsset != null ? case2.sceneAsset.name : case2.sceneName;
        }

        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning("Chat: No se asignó ninguna escena en 'Button Target Scene Asset' o 'Button Target Scene Name'.");
        }
    }

    private IEnumerator Wait(float seconds)
    {
        if (seconds <= 0f) yield break;
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }

    private IEnumerator RunChatFlow()
    {
        // Apagar todo al iniciar para partir limpio
        SetAllObjectsActive(false);

        // 1. Mostrar mensajes del NPC en orden
        for (int i = 0; i < npcMessages.Count; i++)
        {
            TimedMessage msg = npcMessages[i];
            if (msg.messageObject != null)
            {
                msg.messageObject.SetActive(true);
            }
            yield return Wait(msg.delayAfter);
        }

        // 2. Activar opciones a la vez y esperar selección
        if (choiceButtons != null)
        {
            foreach (var btn in choiceButtons)
            {
                if (btn != null) btn.SetActive(true);
            }
        }

        choiceSelected = false;
        yield return new WaitUntil(() => choiceSelected);

        // Ocultar botones de opción una vez elegida una respuesta
        if (choiceButtons != null)
        {
            foreach (var btn in choiceButtons)
            {
                if (btn != null) btn.SetActive(false);
            }
        }

        // 3. Ejecutar el caso elegido
        ChoiceBranch chosenBranch = (selectedCaseIndex == 1) ? case1 : case2;

        if (chosenBranch.responseObjects != null)
        {
            foreach (var resp in chosenBranch.responseObjects)
            {
                if (resp != null) resp.SetActive(true);
            }
        }

        yield return Wait(chosenBranch.delayAfterResponse);

        if (chosenBranch.loadScene)
        {
            string targetScene = chosenBranch.sceneAsset != null ? chosenBranch.sceneAsset.name : chosenBranch.sceneName;
            if (!string.IsNullOrEmpty(targetScene))
            {
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.LogWarning("Chat: Falta asignar la escena en la rama elegida.");
            }
        }
    }

    // Asignar al OnClick del botón del Caso 1 (ej. "Mas tarde")
    public void SelectCase1()
    {
        selectedCaseIndex = 1;
        choiceSelected = true;
    }

    // Asignar al OnClick del botón del Caso 2 (ej. "Pasar")
    public void SelectCase2()
    {
        selectedCaseIndex = 2;
        choiceSelected = true;
    }

    private void SetAllObjectsActive(bool state)
    {
        foreach (var msg in npcMessages)
        {
            if (msg.messageObject != null) msg.messageObject.SetActive(state);
        }

        if (choiceButtons != null)
        {
            foreach (var btn in choiceButtons)
            {
                if (btn != null) btn.SetActive(state);
            }
        }

        SetBranchObjectsActive(case1, state);
        SetBranchObjectsActive(case2, state);
    }

    private void SetBranchObjectsActive(ChoiceBranch branch, bool state)
    {
        if (branch == null || branch.responseObjects == null) return;
        foreach (var obj in branch.responseObjects)
        {
            if (obj != null) obj.SetActive(state);
        }
    }
}