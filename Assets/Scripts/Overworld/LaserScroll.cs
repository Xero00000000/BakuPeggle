using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LaserScroll : MonoBehaviour
{
    [SerializeField] private float scrollSpeedX = 1.0f;
    [SerializeField] private float scrollSpeedY = 0.0f;

    private Image imageComponent;
    private Material customMaterial;

    void Start()
    {
        imageComponent = GetComponent<Image>();

        if (imageComponent.material != null)
        {
            customMaterial = new Material(imageComponent.material);
            imageComponent.material = customMaterial;
        }
    }

    void Update()
    {
        if (customMaterial != null)
        {
            Vector2 offset = customMaterial.mainTextureOffset;
            offset.x += scrollSpeedX * Time.deltaTime;
            offset.y += scrollSpeedY * Time.deltaTime;

            customMaterial.mainTextureOffset = offset;
        }
    }

    void OnDestroy()
    {
        if (customMaterial != null)
        {
            Destroy(customMaterial);
        }
    }
}