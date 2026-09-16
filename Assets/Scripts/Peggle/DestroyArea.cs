using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    [SerializeField] private TemporaryManager manager;
    [SerializeField] bool playerArea;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerArea == true)
        {
            manager.ball1Destroyed = true;
        }
        else
        {
            manager.ball2Destroyed = true;
        }

        Destroy(collision.gameObject);
    }
}
