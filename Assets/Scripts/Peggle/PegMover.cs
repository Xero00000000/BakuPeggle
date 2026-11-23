using UnityEngine;

public class PegMover : MonoBehaviour
{
    public enum LimitMode { Camera, Manual, Collider }

    [Header("Movement")]
    [Tooltip("Tick to make this peg slide left and right. Untick to keep it still.")]
    [SerializeField] private bool move = true;
    [Tooltip("Maximum travel to each side of the start point. Shortened automatically near the limits.")]
    [SerializeField] private float distance = 1.5f;
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private bool randomizePhase = true;

    [Header("Limits")]
    [SerializeField] private LimitMode limitMode = LimitMode.Camera;
    [Tooltip("Camera mode: 0.05 = keep 5% of the screen width free on each side.")]
    [SerializeField, Range(0f, 0.45f)] private float viewportMargin = 0.05f;
    [Tooltip("Manual mode: world X limits (inner edge of the left and right walls).")]
    [SerializeField] private float manualMinX = -5f;
    [SerializeField] private float manualMaxX = 5f;
    [Tooltip("Collider mode: horizontal bounds of this collider are used. It must cover the whole play width, not a single wall.")]
    [SerializeField] private Collider2D areaCollider;
    [Tooltip("Gap kept between the peg's edge and the limit.")]
    [SerializeField] private float edgePadding = 0.05f;

    [Header("Robustness")]
    [Tooltip("If something else moves the peg by more than this, the motion re-centers on the new position.")]
    [SerializeField] private float externalMoveTolerance = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool logInfo = true;

    private Rigidbody2D rb;
    private Vector2 origin;       // x/y only; Z is always read from the transform
    private Vector2 lastTarget;
    private float phase;
    private float halfWidth;
    private bool warned;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        phase = randomizePhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        halfWidth = ComputeHalfWidth();

        origin = transform.position;
        lastTarget = origin;

        if (logInfo)
        {
            bool ok = GetLimits(origin, out float minX, out float maxX);
            Debug.Log($"PegMover '{name}': mode={limitMode}, world pos={transform.position}, " +
                      $"limitsFound={ok}, minX={minX:F2}, maxX={maxX:F2}, halfWidth={halfWidth:F2}", this);
        }
    }

    public void SetMoving(bool value) => move = value;

    public void ResetOrigin()
    {
        origin = rb != null ? rb.position : (Vector2)transform.position;
        lastTarget = origin;
    }

    void FixedUpdate()
    {
        if (!move || rb == null) return;
        rb.MovePosition(Step());          // Rigidbody2D only touches X/Y, Z is untouched
    }

    void Update()
    {
        if (!move || rb != null) return;

        Vector2 p = Step();
        // Keep the peg's own Z (this was the bug: Vector2 -> Vector3 reset Z to 0)
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }

    private Vector2 Step()
    {
        Vector2 current = rb != null ? rb.position : (Vector2)transform.position;

        if (Vector2.Distance(current, lastTarget) > externalMoveTolerance)
            origin += current - lastTarget;

        CalculateRoom(origin, out float left, out float right);

        float s = Mathf.Sin(Time.time * speed + phase);
        float x = origin.x + s * (s >= 0f ? right : left);

        if (GetLimits(origin, out float minX, out float maxX))
        {
            float margin = halfWidth + edgePadding;
            if (minX + margin <= maxX - margin)
                x = Mathf.Clamp(x, minX + margin, maxX - margin);
        }

        lastTarget = new Vector2(x, origin.y);
        return lastTarget;
    }

    private bool GetLimits(Vector2 center, out float minX, out float maxX)
    {
        minX = maxX = 0f;

        switch (limitMode)
        {
            case LimitMode.Camera:
                {
                    Camera cam = Camera.main;
                    if (cam == null) { WarnOnce("No camera tagged MainCamera found."); return false; }

                    float z = transform.position.z;
                    Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, z));
                    float vy = Mathf.Clamp01(cam.WorldToViewportPoint(new Vector3(center.x, center.y, z)).y);

                    if (!XOnPlane(cam, plane, viewportMargin, vy, out minX) ||
                        !XOnPlane(cam, plane, 1f - viewportMargin, vy, out maxX))
                    {
                        WarnOnce("Camera ray never hits the peg's plane.");
                        return false;
                    }
                    if (minX > maxX) { float t = minX; minX = maxX; maxX = t; }
                    return true;
                }
            case LimitMode.Manual:
                minX = manualMinX;
                maxX = manualMaxX;
                return true;
            case LimitMode.Collider:
                if (areaCollider == null) { WarnOnce("Collider mode but no Area Collider assigned."); return false; }
                minX = areaCollider.bounds.min.x;
                maxX = areaCollider.bounds.max.x;
                return true;
        }
        return false;
    }

    private static bool XOnPlane(Camera cam, Plane plane, float vx, float vy, out float x)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(vx, vy, 0f));
        if (plane.Raycast(ray, out float d)) { x = ray.GetPoint(d).x; return true; }
        x = 0f;
        return false;
    }

    private void CalculateRoom(Vector2 center, out float left, out float right)
    {
        left = distance;
        right = distance;

        if (!GetLimits(center, out float minX, out float maxX)) return;

        float margin = halfWidth + edgePadding;
        left = Mathf.Clamp(center.x - (minX + margin), 0f, distance);
        right = Mathf.Clamp((maxX - margin) - center.x, 0f, distance);
    }

    private float ComputeHalfWidth()
    {
        Collider2D own = GetComponent<Collider2D>();
        if (own != null) return own.bounds.extents.x;
        Renderer r = GetComponent<Renderer>();
        return r != null ? r.bounds.extents.x : 0f;
    }

    private void WarnOnce(string msg)
    {
        if (warned) return;
        warned = true;
        Debug.LogWarning($"PegMover '{name}': {msg} Falling back to plain Distance.", this);
    }

    void OnDrawGizmosSelected()
    {
        if (!move) return;
        if (halfWidth <= 0f) halfWidth = ComputeHalfWidth();

        Vector2 c2 = Application.isPlaying ? origin : (Vector2)transform.position;
        CalculateRoom(c2, out float l, out float r);

        // Use the peg's real Z so the line is drawn on the peg, not on Z=0
        float z = transform.position.z;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(c2.x - l, c2.y, z), new Vector3(c2.x + r, c2.y, z));
    }
}