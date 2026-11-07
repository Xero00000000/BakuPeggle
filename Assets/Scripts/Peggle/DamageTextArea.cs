using UnityEngine;

public class DamageTextArea : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(10f, 8f);

    public Rect WorldRect
    {
        get
        {
            Vector2 s = new Vector2(size.x * Mathf.Abs(transform.lossyScale.x),
                                    size.y * Mathf.Abs(transform.lossyScale.y));
            Vector2 c = transform.position;
            return new Rect(c - s * 0.5f, s);
        }
    }

    private void OnDrawGizmos()
    {
        Rect r = WorldRect;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(r.center, new Vector3(r.width, r.height, 0.1f));
    }
}