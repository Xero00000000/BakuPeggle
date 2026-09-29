using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    [SerializeField] private TemporaryManager manager;
    [SerializeField] bool playerArea;

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
