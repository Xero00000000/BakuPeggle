using UnityEngine;
using UnityEngine.UI;

public class TravelingNeonBorder : MonoBehaviour
{
    [Header("Orden: Arriba, Derecha, Abajo, Izquierda")]
    [SerializeField] private Image[] borderSegments;

    [SerializeField] private float speed = 3f;
    [SerializeField] private Color brightColor = new Color(0f, 1f, 0.4f, 1f); 
    [SerializeField] private Color dimColor = new Color(0f, 0.2f, 0.1f, 0.3f);    

    private float currentIndex = 0f;

    void Update()
    {
        if (borderSegments == null || borderSegments.Length == 0) return;

        currentIndex = (currentIndex + Time.deltaTime * speed) % borderSegments.Length;
        int activeIndex = Mathf.FloorToInt(currentIndex);
        float t = currentIndex - activeIndex;

        for (int i = 0; i < borderSegments.Length; i++)
        {
            if (borderSegments[i] == null) continue;

            int nextIndex = (i + 1) % borderSegments.Length;

            if (i == activeIndex)
            {
                borderSegments[i].color = Color.Lerp(brightColor, dimColor, t);
            }
            else if (i == nextIndex)
            {
                borderSegments[i].color = Color.Lerp(dimColor, brightColor, t);
            }
            else
            {
                borderSegments[i].color = dimColor;
            }
        }
    }
}