using UnityEngine;

public class PegLauncher : MonoBehaviour
{
    public Vector3 mousePos;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float force;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(mousePos.x, mousePos.z, mousePos.y) - transform.position;
        //direction.y = 0f;


        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0f, 0f, lookRotation.eulerAngles.y);
        }

        if (Input.GetMouseButtonDown(0))
        {
            var instance = Instantiate(ballPrefab, spawnPoint.transform.position, Quaternion.identity);
            instance.GetComponent<Rigidbody2D>().AddForce((direction) * force);
        }
    }
}
