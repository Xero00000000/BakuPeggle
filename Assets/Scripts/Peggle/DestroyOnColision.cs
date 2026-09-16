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
}
