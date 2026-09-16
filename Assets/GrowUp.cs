using UnityEngine;
using UnityEngine.EventSystems;

public class GrowUp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Crecimiento")]
    [SerializeField] private float growAmount = 0.2f;
    [SerializeField] private float speed = 10f;

    private Vector3 initialScale;
    private Vector3 targetScale;

    private void Awake()
    {
        initialScale = transform.localScale;
        targetScale = initialScale;
    }

    private void OnDisable()
    {
        transform.localScale = initialScale;
        targetScale = initialScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = initialScale + new Vector3(growAmount, growAmount, growAmount);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = initialScale;
    }
}