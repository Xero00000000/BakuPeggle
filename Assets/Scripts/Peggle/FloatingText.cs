using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Referencias de Texto")]
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private TextMeshPro shadowTextMesh;

    [Header("Configuración de la Sombra")]
    [SerializeField] private Color shadowColor = Color.black;
    [Tooltip("Desplazamiento local de la sombra respecto al texto principal.")]
    [SerializeField] private Vector3 shadowOffset = new Vector3(0.03f, -0.03f, 0.01f);

    [Header("Configuración de Animación")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    public void Setup(string text, Color color)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
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
            }
        }

        Destroy(gameObject, fadeDuration);
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a -= Time.deltaTime / fadeDuration;
            textMesh.color = c;
        }
        if (shadowTextMesh != null)
        {
            Color sc = shadowTextMesh.color;
            sc.a -= Time.deltaTime / fadeDuration;
            shadowTextMesh.color = sc;
        }
    }
}