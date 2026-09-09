using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PegLauncher)/*, typeof(Player)*/)]
public class TargettingManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private Vector3 mousePos;
    [SerializeField] private LayerMask mouseDetectionLayer;
    [SerializeField] private PegLauncher pegLauncher;
    //[SerializeField] private Player player;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, mouseDetectionLayer))
        {
            mousePos = raycastHit.point;

            Vector3 direction = mousePos - transform.position;
            direction.z = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Euler(0f, 0f, lookRotation.eulerAngles.z);
            }
        }
    }
}
