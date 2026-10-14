using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ConditionalText : MonoBehaviour
{
    [System.Serializable]
    public struct TextCondition
    {
        public GameObject targetObject;
        [TextArea(2, 4)]
        public string descriptionText;
        public string costText;
    }
    [System.Serializable]
    public struct ComponentCondition
    {
        public GameObject targetGameObject;
        [Header("Elige una condición basada en un script existente")]
        public ResizableItem targetResizableItem;

        public bool requiredBoolValue;
        [TextArea(2, 4)] public string descriptionText;
        public string costText;
    }

    [Header("Arrays de TextMeshPro a modificar")]
    [SerializeField] private List<TextMeshProUGUI> targetTextUIList = new List<TextMeshProUGUI>(); 
    [SerializeField] private List<TextMeshProUGUI> costTextUIList = new List<TextMeshProUGUI>();   

    [Header("Condiciones basadas en Propiedades (Booleans)")]
    [SerializeField] private List<ComponentCondition> conditions = new List<ComponentCondition>();

    [Header("Textos por defecto (Si ninguna condición se cumple)")]
    [SerializeField] private string defaultDescriptionText = "";
    [SerializeField] private string defaultCostText = "";

    void Update()
    {
        EvaluateConditions();
    }

    private void EvaluateConditions()
    {
        string descToApply = defaultDescriptionText;
        string costToApply = defaultCostText;

        foreach (var cond in conditions)
        {
            if (cond.targetResizableItem != null)
            {
                if (cond.targetResizableItem.isSelected == cond.requiredBoolValue)
                {
                    descToApply = cond.descriptionText;
                    costToApply = cond.costText;
                    break; 
                }
            }
        }

        foreach (var descUI in targetTextUIList)
        {
            if (descUI != null)
            {
                descUI.text = descToApply;
            }
        }
        foreach (var costUI in costTextUIList)
        {
            if (costUI != null)
            {
                costUI.text = costToApply;
            }
        }
    }
}