using UnityEngine;

public class DestroyOnColision : MonoBehaviour
{
    [SerializeField] private bool playerPeg;
    
    private Renderer pegRenderer;
    [SerializeField] private Material[] types;
    [SerializeField] private int currentType;

    [SerializeField] private PointsEventChannel _addPoints;

    public void Start()
    {
        pegRenderer = GetComponent<Renderer>();
        pegRenderer.material = types[currentType];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _addPoints.Raise(this, playerPeg, currentType);

        Destroy(gameObject);
    }

    public Color PegColor
    {
        get
        {
            if (pegRenderer == null) pegRenderer = GetComponent<Renderer>();
            Material m = pegRenderer.sharedMaterial;
            if (m == null) return Color.white;
            if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor"); // URP Lit
            if (m.HasProperty("_Color")) return m.GetColor("_Color");     // Standard / unlit
            return Color.white;
        }
    }
}
