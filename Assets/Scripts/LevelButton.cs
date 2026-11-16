using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [SerializeField] private int levelNumber = 1;

    [Header("Scene loading")]
    [Tooltip("The SceneChanger on your SceneManager object.")]
    [SerializeField] private SceneChanger sceneChanger;
    [SerializeField] private SceneField[] scenesToLoad;
    [SerializeField] private SceneField[] scenesToUnload;

    [Header("Optional")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private string unlockedText = "Level {0}";
    [SerializeField] private string lockedText = "Locked";
    [Tooltip("Shown only while the level is locked, e.g. a padlock image.")]
    [SerializeField] private GameObject lockIcon;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LoadLevel);
    }

    // Runs every time the LevelCanvas opens, so it always shows the latest progress
    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (button == null) button = GetComponent<Button>();

        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        button.interactable = unlocked;

        if (label != null)
            label.text = unlocked ? string.Format(unlockedText, levelNumber) : lockedText;

        if (lockIcon != null) lockIcon.SetActive(!unlocked);
    }

    private void LoadLevel()
    {
        if (!LevelProgress.IsUnlocked(levelNumber)) return;
        if (sceneChanger == null) { Debug.LogWarning($"{name}: no SceneChanger assigned.", this); return; }

        Time.timeScale = 1f;
        sceneChanger.LoadScenes(scenesToLoad, scenesToUnload);
    }
}