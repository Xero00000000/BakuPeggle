using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TemporaryManager : MonoBehaviour
{
    private Camera pegCamera;
    private Vector3 mousePos;

    [SerializeField] private GameObject ballPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pegCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = pegCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector3 rotation = mousePos - transform.position;

        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(ballPrefab, transform.position, Quaternion.identity);
        }
    }
}
