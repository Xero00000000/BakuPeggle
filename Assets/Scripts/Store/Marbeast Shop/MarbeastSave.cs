using UnityEngine;

public static class MarbeastSave
{
    public const string COINS_KEY = "PlayerCoins";
    public const int DEFAULT_COINS = 500;

    const string UNLOCK_PREFIX = "Marbeast_";
    const string EQUIPPED_ID_KEY = "EquippedMarbeastId";
    public const string EQUIPPED_INDEX_KEY = "EquippedBallIndex";
    public static event System.Action OnReset;

    public static event System.Action<int> OnEquipChanged;
    public static int Coins
    {
        get => PlayerPrefs.GetInt(COINS_KEY, DEFAULT_COINS);
        set => PlayerPrefs.SetInt(COINS_KEY, value);
    }

    public static bool IsUnlocked(MarbeastItem item) =>
        item.unlockedByDefault || PlayerPrefs.GetInt(UNLOCK_PREFIX + item.marbeastId, 0) == 1;

    public static bool IsEquipped(MarbeastItem item) =>
        PlayerPrefs.GetString(EQUIPPED_ID_KEY, "") == item.marbeastId;

    public static bool TryBuy(MarbeastItem item)
    {
        if (IsUnlocked(item)) return true;
        if (Coins < item.price) return false;

        Coins -= item.price;
        PlayerPrefs.SetInt(UNLOCK_PREFIX + item.marbeastId, 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void Equip(MarbeastItem item)
    {
        PlayerPrefs.SetString(EQUIPPED_ID_KEY, item.marbeastId);
        PlayerPrefs.SetInt(EQUIPPED_INDEX_KEY, item.ballIndex);
        PlayerPrefs.Save();
        OnEquipChanged?.Invoke(item.ballIndex);
    }

    public static void ResetPurchases(MarbeastItem[] items, bool resetCoins = true)
    {
        foreach (var item in items)
            PlayerPrefs.DeleteKey(UNLOCK_PREFIX + item.marbeastId);

        PlayerPrefs.DeleteKey(EQUIPPED_ID_KEY);
        PlayerPrefs.DeleteKey(EQUIPPED_INDEX_KEY);

        if (resetCoins)
            PlayerPrefs.SetInt(COINS_KEY, DEFAULT_COINS);

        PlayerPrefs.Save();

        OnEquipChanged?.Invoke(0);   // launcher goes back to the normal ball
        OnReset?.Invoke();           // cards refresh
    }
}