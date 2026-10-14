using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ResizableItem : MonoBehaviour, IPointerClickHandler
{
    private Vector3 normalScale;
    private Vector3 targetScale;
    private float scaleSpeed = 10f;
    public bool isSelected = false;

    [Header("Referencias")]
    [SerializeField] private ItemManager manager;

    [Header("Configuración de Tamaño Personalizado")]
    [SerializeField] private float desireScaleXZ = 1.25f;
    [SerializeField] private float desireScaleY = 1.25f;

    [Header("Estado Inicial")]
    [SerializeField] public bool isDefaultLarge = false; // <--- Casilla para arrancar grande

    public event Action<bool> OnSelectionChanged;

    void Start()
    {
        normalScale = transform.localScale;

        // Si está marcado como default grande, arranca directamente con el tamaño deseado
        if (isDefaultLarge)
        {
            isSelected = true;
            targetScale = new Vector3(desireScaleXZ, desireScaleY, transform.localScale.z);
            transform.localScale = targetScale; // Aplica el tamaño al instante al iniciar
        }
        else
        {
            targetScale = normalScale;
        }

        if (manager == null)
        {
            manager = FindFirstObjectByType<ItemManager>();
        }
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.SelectOnlyThis(this);
        }
    }
    public void MakeLarge()
    {
        if (!isSelected)
        {
            isSelected = true;
            targetScale = new Vector3(desireScaleXZ, desireScaleY, transform.localScale.z);
            OnSelectionChanged?.Invoke(isSelected);
        }
    }

    public void MakeNormal()
    {
        if (isSelected)
        {
            isSelected = false;
            targetScale = normalScale;
            OnSelectionChanged?.Invoke(isSelected);
        }
    }
}