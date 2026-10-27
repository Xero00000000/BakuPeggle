using System.Collections.Generic;

using UnityEngine;



[RequireComponent(typeof(LineRenderer))]

public class AimTrajectoryPredictor2D : MonoBehaviour

{

    [Header("Referencias")]

    [SerializeField] private Transform spawnPoint; // El objeto exacto desde donde querés que salga



    [Header("Configuración")]

    [SerializeField] private float ballRadius = 0.35f;

    [SerializeField] private float maxDistance = 20f;

    [Range(1, 5)][SerializeField] private int maxBounces = 1;

    [SerializeField] private LayerMask collisionMask = ~0;



    private LineRenderer lineRenderer;

    private readonly List<Vector3> points = new List<Vector3>();



    void Awake()

    {

        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;



        if (lineRenderer.sharedMaterial == null || lineRenderer.sharedMaterial.name == "Default-Material")

        {

            lineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

        }



        lineRenderer.startWidth = 0.2f;

        lineRenderer.endWidth = 0.2f;

        lineRenderer.startColor = Color.red;

        lineRenderer.endColor = Color.red;

        lineRenderer.sortingOrder = 32767;

    }



    void LateUpdate()

    {

        lineRenderer.enabled = true;



        if (spawnPoint == null)

        {

            Debug.LogError("[AimTrajectory] ¡Falta asignar el spawnPoint en el Inspector!");

            return;

        }



        // Como el Canvas está en World Space, .position ya nos da la ubicación exacta en el mundo 3D

        Vector3 worldOrigin = spawnPoint.position;

        worldOrigin.z = 0f; // Aseguramos el plano 2D de las físicas



        // Dirección de disparo basada en la rotación de la torreta

        Vector2 shootDirection = -transform.up;



        DrawTrajectory((Vector2)worldOrigin, shootDirection, 0f);

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

                if (hit.collider.attachedRigidbody != null && hit.collider.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)

                {

                    currentPos = hit.point + currentDir * 0.1f;

                    continue;

                }



                Vector2 impactPoint = hit.centroid;

                points.Add(new Vector3(impactPoint.x, impactPoint.y, zDepth));



                remainingDistance -= hit.distance;

                if (remainingDistance <= 0.2f) break;



                currentDir = Vector2.Reflect(currentDir, hit.normal).normalized;

                currentPos = impactPoint + currentDir * 0.02f;

            }

            else

            {

                Vector2 endPosition = currentPos + (currentDir * remainingDistance);

                points.Add(new Vector3(endPosition.x, endPosition.y, zDepth));

                break;

            }

        }



        lineRenderer.positionCount = points.Count;

        lineRenderer.SetPositions(points.ToArray());

    }

}