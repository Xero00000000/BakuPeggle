using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TemporaryManager : MonoBehaviour
{
    private Camera pegCamera;
    private Vector3 mousePos;
    [SerializeField] private LayerMask mouseDetectionLayer;

    [SerializeField] private GameObject ballPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pegCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        mousePos = pegCamera.ScreenToWorldPoint(Input.mousePosition);

        mousePos.z = -(transform.position.x - pegCamera.transform.position.x);

        Vector3 rotation = mousePos - transform.position;

        float rotX = Mathf.Atan2(rotation.z, rotation.y) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(rotX, 0, 0);

        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(ballPrefab, transform.position, Quaternion.identity);
        }
        */

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, mouseDetectionLayer))
        {
            mousePos = raycastHit.point;

            Vector3 direction = mousePos - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
            }
        }
    }
}
