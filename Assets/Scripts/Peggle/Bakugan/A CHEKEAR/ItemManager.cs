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

    private int currentIndex = 0;

    void Start()
    {
        if (items.Count == 0)
        {
            items.AddRange(FindObjectsByType<ResizableItem>(FindObjectsSortMode.None));
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].isDefaultLarge)
            {
                currentIndex = i;
                break;
            }
        }

        UpdateSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex = items.Count - 1;
            }
            else
            {
                currentIndex++;
                if (currentIndex >= items.Count) currentIndex = 0;
            }

            UpdateSelection();
        }
    }

    public void SelectOnlyThis(ResizableItem selectedItem)
    {
        int index = items.IndexOf(selectedItem);
        if (index != -1)
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
}