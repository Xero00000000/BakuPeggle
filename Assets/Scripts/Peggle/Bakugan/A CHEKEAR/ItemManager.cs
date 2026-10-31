using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("Lista de Items Seleccionables")]
    [SerializeField] private List<ResizableItem> items = new List<ResizableItem>();

    [Header("Referencias Externe")]
    [SerializeField] private TurnManager temporaryManager;
    [SerializeField] private PeggleUIManager UIManager;
    [SerializeField] private PegLauncher launcher;

    [Header("Shop link")]
    [SerializeField] private MarbeastItem[] marbeasts;   // drag all MarbeastItem assets here

    private int currentIndex = 0;

    void Start()
    {
        if (items.Count == 0)
            items.AddRange(FindObjectsByType<ResizableItem>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].isDefaultLarge)
            {
                currentIndex = i;
                break;
            }
        }

        RefreshAvailability();
        if (!IsAvailable(currentIndex)) currentIndex = FirstAvailable();

        UpdateSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool back = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            currentIndex = Step(currentIndex, back ? -1 : 1);
            UpdateSelection();
        }
    }

    public void SelectOnlyThis(ResizableItem selectedItem)
    {

        int index = items.IndexOf(selectedItem);
        if (index != -1 && IsAvailable(index))
        {
            currentIndex = index;
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        if (items.Count == 0) return;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;

            if (i == currentIndex)
            {
                items[i].MakeLarge(); 

                if (temporaryManager != null)
                {
                    switch (i)
                    {
                        case 0:
                            temporaryManager.SetDamageAbility();
                            break;
                        case 1:
                            temporaryManager.SetHealAbility();
                            break;
                        case 2:
                            temporaryManager.SetCloneAbility();
                            break;
                        case 3:
                            temporaryManager.SetBombAbility();
                            break;
                    };
                }
                if (UIManager != null)
                {
                    if (i == 0 || i == 2)
                    {
                        UIManager.SetDamageAbility();
                    }
                    else if (i == 1 || i == 3)
                    {
                        UIManager.SetHealAbility();
                    }
                }
                if (launcher != null)
                {
                    switch (i)
                    {
                        case 0:
                            launcher.ChangeBall(0);
                            break;
                        case 1:
                            launcher.ChangeBall(1);
                            break;
                        case 2:
                            launcher.ChangeBall(2);
                            break;
                        case 3:
                            launcher.ChangeBall(3);
                            break;
                    };
                }
            }
            else
            {
                items[i].MakeNormal(); 
            }
        }
    }

    void OnEnable()
    {
        MarbeastSave.OnEquipChanged += HandleEquip;
    }

    void OnDisable()
    {
        MarbeastSave.OnEquipChanged -= HandleEquip;
    }

    // Is the item at this list index available to the player?
    private bool IsAvailable(int index)
    {
        foreach (var m in marbeasts)
            if (m != null && m.ballIndex == index)
                return MarbeastSave.IsUnlocked(m);

        return true;   // no marbeast asset for this slot -> always available
    }

    // Show unlocked items, hide locked ones
    private void RefreshAvailability()
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i] != null)
                items[i].gameObject.SetActive(IsAvailable(i));
    }

    // Fires when the shop buys/equips (or resets)
    private void HandleEquip(int ballIndex)
    {
        RefreshAvailability();

        if (ballIndex >= 0 && ballIndex < items.Count && IsAvailable(ballIndex))
            currentIndex = ballIndex;
        else
            currentIndex = FirstAvailable();

        UpdateSelection();
    }

    private int FirstAvailable()
    {
        for (int i = 0; i < items.Count; i++)
            if (IsAvailable(i)) return i;
        return 0;
    }

    // Next/previous available index, skipping locked ones
    private int Step(int from, int dir)
    {
        for (int n = 0; n < items.Count; n++)
        {
            from = (from + dir + items.Count) % items.Count;
            if (IsAvailable(from)) return from;
        }
        return from;
    }
}