using UnityEngine;

public static class LevelProgress
{
    private const string KEY = "HighestUnlockedLevel";

    // Level 1 is always unlocked
    public static int HighestUnlocked => Mathf.Max(1, PlayerPrefs.GetInt(KEY, 1));

    public static bool IsUnlocked(int level) => level <= HighestUnlocked;

    // Call when the player wins a level. Unlocks the next one.
    public static void CompleteLevel(int level)
    {
        if (level < HighestUnlocked) return;   // replaying an old level changes nothing

        PlayerPrefs.SetInt(KEY, level + 1);
        PlayerPrefs.Save();
    }

    public static void SetHighestUnlocked(int level)
    {
        PlayerPrefs.SetInt(KEY, Mathf.Max(1, level));
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY);
        PlayerPrefs.Save();
    }
}