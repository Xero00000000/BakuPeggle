using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Canvas y UI de Pausa")]
    [Tooltip("Arrastra aquí el Canvas entero que quieres activar/desactivar al pausar y reanudar.")]
    [SerializeField] private GameObject targetCanvas;
    [SerializeField] private GameObject pausePanel;
    [Tooltip("Opcional: asigna aquí los botones si están fuera de pausePanel")]
    [SerializeField] private Selectable[] extraPauseSelectables;

    [Header("Filtro por Escena")]
    [Tooltip("Nombre exacto de la escena a pausar. Vacío = cualquier escena activa.")]
    [SerializeField] private string targetSceneName = "";

    [Header("Tecla de Pausa")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Cursor (útil en juegos 3D)")]
    [SerializeField] private bool unlockCursorOnPause = true;

    [Header("Salida")]
    [Tooltip("Si se completa, carga esta escena al salir. Si queda vacío, cierra la aplicación.")]
    [SerializeField] private string exitSceneName = "";

    private bool isPaused = false;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;
    private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();
    private readonly List<bool> previousBehaviourStates = new List<bool>();

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPauseUIActive(false);
    }

    void Update()
    {
        if (!IsTargetSceneActive()) return;

        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private bool IsTargetSceneActive()
    {
        if (string.IsNullOrEmpty(targetSceneName)) return true;
        return SceneManager.GetActiveScene().name == targetSceneName;
    }

    private void SetPauseUIActive(bool active)
    {
        if (targetCanvas != null)
            targetCanvas.SetActive(active);

        if (pausePanel != null)
            pausePanel.SetActive(active);

        if (extraPauseSelectables != null)
        {
            foreach (var sel in extraPauseSelectables)
            {
                if (sel != null) sel.gameObject.SetActive(active);
            }
        }
    }

    public void PauseGame()
    {
        if (isPaused || !IsTargetSceneActive()) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (unlockCursorOnPause)
        {
            previousLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        DisableSceneGameplayBehaviours();
        SetPauseUIActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;

        SetPauseUIActive(false);
        RestoreSceneBehaviours();

        if (unlockCursorOnPause)
        {
            Cursor.lockState = previousLockMode;
            Cursor.visible = previousCursorVisible;
        }
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (unlockCursorOnPause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        RestoreSceneBehaviours();

        if (!string.IsNullOrEmpty(exitSceneName))
        {
            SceneManager.LoadScene(exitSceneName);
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private bool IsPartOfPauseUI(Component target)
    {
        if (target == null) return false;

        if (targetCanvas != null && (target.gameObject == targetCanvas || target.transform.IsChildOf(targetCanvas.transform) || targetCanvas.transform.IsChildOf(target.transform)))
        {
            return true;
        }

        if (pausePanel != null && (target.gameObject == pausePanel || target.transform.IsChildOf(pausePanel.transform) || pausePanel.transform.IsChildOf(target.transform)))
        {
            return true;
        }

        if (extraPauseSelectables != null)
        {
            foreach (var sel in extraPauseSelectables)
            {
                if (sel != null && (target.gameObject == sel.gameObject || target.transform.IsChildOf(sel.transform)))
                {
                    return true;
                }
            }
        }

        return target.gameObject == gameObject || target.transform.IsChildOf(transform);
    }

    private bool ShouldKeepBehaviourEnabled(Behaviour comp)
    {
        if (comp == null || comp == this) return true;
        if (IsPartOfPauseUI(comp)) return true;
        if (comp is Camera || comp is EventSystem || comp is BaseInputModule || comp is Canvas || comp is CanvasScaler || comp is GraphicRaycaster || comp is Graphic || comp is Selectable)
        {
            return true;
        }
        return false;
    }

    private void DisableSceneGameplayBehaviours()
    {
        disabledBehaviours.Clear();
        previousBehaviourStates.Clear();

        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject root = rootObjects[i];
            if (root == null) continue;

            Behaviour[] behaviours = root.GetComponentsInChildren<Behaviour>(true);
            for (int j = 0; j < behaviours.Length; j++)
            {
                Behaviour comp = behaviours[j];
                if (ShouldKeepBehaviourEnabled(comp)) continue;

                disabledBehaviours.Add(comp);
                previousBehaviourStates.Add(comp.enabled);
                comp.enabled = false;
            }
        }
    }

    private void RestoreSceneBehaviours()
    {
        for (int i = 0; i < disabledBehaviours.Count; i++)
        {
            Behaviour comp = disabledBehaviours[i];
            if (comp != null)
            {
                comp.enabled = previousBehaviourStates[i];
            }
        }

        disabledBehaviours.Clear();
        previousBehaviourStates.Clear();
    }

    void OnDestroy()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            RestoreSceneBehaviours();
        }
    }
}