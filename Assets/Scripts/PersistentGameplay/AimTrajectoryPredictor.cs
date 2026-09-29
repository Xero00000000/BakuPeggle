using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimTrajectoryPredictor : MonoBehaviour
{
    [Header("Referencias")]
    public PegLauncher launcher;
    public Transform spawnPoint;

    [Header("Configuración")]
    public float ballRadius = 0.35f;
    public float maxDistance = 20f;
    [Range(1, 2)] public int maxBounces = 1;

    [Header("Filtros")]
    [Tooltip("Capas con las que choca (Paredes y Pegs)")]
    public LayerMask collisionMask = ~0;

    private LineRenderer lineRenderer;
    private readonly List<Vector3> points = new List<Vector3>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;

        if (launcher == null)
            launcher = GetComponentInParent<PegLauncher>();
    }

    void LateUpdate()
    {
        if (launcher == null) return;

        // Aseguramos que esté visible
        lineRenderer.enabled = true;

        // Origen en el spawnPoint
        Vector3 originPos = spawnPoint != null ? spawnPoint.position : launcher.transform.position;
        float zDepth = originPos.z;

        // Dirección directa hacia donde apunta el cañón
        // En Unity 2D con rotación Z, el cañón rota sobre el eje transform.right o -transform.up
        Vector2 shootDirection = launcher.transform.right;

        DrawTrajectory((Vector2)originPos, shootDirection, zDepth);
    }

    private void DrawTrajectory(Vector2 startPos, Vector2 startDir, float zDepth)
    {
        points.Clear();
        points.Add(new Vector3(startPos.x, startPos.y, zDepth));

        Vector2 currentPos = startPos;
        Vector2 currentDir = startDir.normalized;
        float remainingDistance = maxDistance;

        for (int i = 0; i < maxBounces; i++)
        {
            // Salteamos el collider del cañón arrancando un toque más adelante
            Vector2 rayOrigin = currentPos + currentDir * (ballRadius + 0.1f);

            RaycastHit2D hit = Physics2D.CircleCast(
                rayOrigin,
                ballRadius,
                currentDir,
                remainingDistance,
                collisionMask
            );

            if (hit.collider != null && hit.fraction > 0)
            {
                // Si choca con un rigidbody dinámico (la bola gris ya disparada), la salteamos
                if (hit.collider.attachedRigidbody != null && hit.collider.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
                {
                    currentPos = hit.point + currentDir * 0.1f;
                    continue;
                }

                Vector2 impactPoint = hit.centroid;
                points.Add(new Vector3(impactPoint.x, impactPoint.y, zDepth));

                remainingDistance -= hit.distance;
                if (remainingDistance <= 0.2f) break;

                // Rebote
                currentDir = Vector2.Reflect(currentDir, hit.normal).normalized;
                currentPos = impactPoint + currentDir * 0.02f;
            }
            else
            {
                Vector2 endPoint = currentPos + currentDir * remainingDistance;
                points.Add(new Vector3(endPoint.x, endPoint.y, zDepth));
                break;
            }
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}