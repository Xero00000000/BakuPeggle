using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HorizontalScrollSnap : MonoBehaviour, IEndDragHandler, IDragHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private float snapSpeed = 10f;

    private List<float> pagePositions = new List<float>();
    private int pageCount;
    private int targetPage;
    private bool isDragging;

    void Start()
    {

        pageCount = content.childCount;

        if (pageCount <= 1) return;

        for (int i = 0; i < pageCount; i++)
        {
            pagePositions.Add((float)i / (pageCount - 1));
        }
    }

    void Update()
    {
        if (!isDragging && pageCount > 1)
        {
            float currentPos = scrollRect.horizontalNormalizedPosition;
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(currentPos, pagePositions[targetPage], Time.deltaTime * snapSpeed);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        float currentPos = scrollRect.horizontalNormalizedPosition;
        float closestDistance = float.MaxValue;
        int closestPage = 0;

        for (int i = 0; i < pagePositions.Count; i++)
        {
            float distance = Mathf.Abs(currentPos - pagePositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPage = i;
            }
        }

        targetPage = closestPage;
    }
}
