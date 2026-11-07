using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Referencias de Texto")]
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private TextMeshPro shadowTextMesh;

    [Header("Configuración de la Sombra")]
    [SerializeField] private Color shadowColor = Color.black;
    [SerializeField] private Vector3 shadowOffset = new Vector3(0.03f, -0.03f, 0.01f);

    [Header("Configuración de Animación")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private float timeLeft;
    private Color mainColor = Color.white;
    private int baseSortingOrder;
    private bool sortingCaptured;

    public void Setup(string text, Color color)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        mainColor = color;

        // Capture the prefab's sorting order once, so repeated Setup calls don't compound it
        if (!sortingCaptured && textMesh != null)
        {
            baseSortingOrder = textMesh.sortingOrder;
            sortingCaptured = true;
        }

        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textMesh.sortingOrder = baseSortingOrder + 1;   // main text above the shadow
        }

        if (shadowTextMesh != null)
        {
            shadowTextMesh.text = text;
            shadowTextMesh.color = shadowColor;
            shadowTextMesh.transform.localPosition = shadowOffset;
            if (textMesh != null)
            {
                shadowTextMesh.fontSize = textMesh.fontSize;
                shadowTextMesh.alignment = textMesh.alignment;
                shadowTextMesh.fontStyle = textMesh.fontStyle;
                shadowTextMesh.sortingLayerID = textMesh.sortingLayerID;
                shadowTextMesh.sortingOrder = baseSortingOrder;   // shadow keeps the original order
            }
        }

        timeLeft = fadeDuration;
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timeLeft -= Time.deltaTime;
        float a = Mathf.Clamp01(timeLeft / fadeDuration);

        if (textMesh != null)
        {
            Color c = mainColor; c.a = mainColor.a * a;
            textMesh.color = c;
        }
        if (shadowTextMesh != null)
        {
            Color sc = shadowColor; sc.a = shadowColor.a * a;
            shadowTextMesh.color = sc;
        }

        if (timeLeft <= 0f) Destroy(gameObject);
    }
}