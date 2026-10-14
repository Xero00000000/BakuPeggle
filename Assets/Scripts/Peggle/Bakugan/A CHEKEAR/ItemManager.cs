using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("Lista de Items Seleccionables")]
    [SerializeField] private List<ResizableItem> items = new List<ResizableItem>();

    [Header("Referencias Externe")]
    [SerializeField] private TemporaryManager temporaryManager;

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
                    if (i == 0)
                    {
                        temporaryManager.SetDamageAbility();
                    }
                    else if (i == 1)
                    {
                        temporaryManager.SetHealAbility();
                    }
                }
            }
            else
            {
                items[i].MakeNormal(); 
            }
        }
    }
}