using UnityEngine;
using UnityEngine.EventSystems;

public class TextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Objeto a Activar/Desactivar con Hover")]
    [SerializeField] private GameObject objectToToggle;
    [SerializeField] private bool hideOnStart = true;

    private void Start()
    {
        if (hideOnStart && objectToToggle != null)
        {
            objectToToggle.SetActive(false);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (objectToToggle != null)
        {
            objectToToggle.SetActive(true);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (objectToToggle != null)
        {
            objectToToggle.SetActive(false);
        }
    }
    private void OnDisable()
    {
        if (objectToToggle != null)
        {
            objectToToggle.SetActive(false);
        }
    }
}