using UnityEngine;
using UnityEngine.SceneManagement;

public class PhoneAutoHider : MonoBehaviour
{
    [Header("Referencias del Telefono")]
    [SerializeField] private GameObject phoneMiniButton;
    [SerializeField] private GameObject phoneFull;

    [Header("Tecla de Transicion")]
    [SerializeField] private KeyCode transitionKey = KeyCode.Escape;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (Input.GetKeyDown(transitionKey))
        {
            HideEverything();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ShowMiniButtonOnly();
    }

    public void HideEverything()
    {
        if (phoneFull != null)
            phoneFull.SetActive(false);

        if (phoneMiniButton != null)
            phoneMiniButton.SetActive(false);
    }

    public void ShowMiniButtonOnly()
    {
        if (phoneFull != null)
            phoneFull.SetActive(false); 

        if (phoneMiniButton != null)
            phoneMiniButton.SetActive(true); 
    }
}