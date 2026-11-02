using UnityEngine;
using UnityEngine.Events;

public class LoadScenes : MonoBehaviour
{
    [SerializeField] private SceneChangeEventChannel eventToBroadcast;
    [SerializeField] private SceneField[] scenesToLoad;
    [SerializeField] private SceneField[] scenesToUnload;

    [Header("UI to hide when changing scene")]
    [SerializeField] private GameObject[] objectsToHide;      // drag WinCanvas / LoseCanvas here
    [SerializeField] private UnityEvent onBeforeChange;       // optional extra hooks

    private bool raised;

    void OnEnable() { raised = false; }

    public void RaiseEvent()
    {
        if (raised) return;          // prevents double clicks requesting twice
        raised = true;

        Time.timeScale = 1f;         // never carry a frozen game into the next scene
        eventToBroadcast.Raise(this, scenesToLoad, scenesToUnload, null);

        foreach (var go in objectsToHide)
            if (go != null) go.SetActive(false);

        onBeforeChange?.Invoke();
    }
}