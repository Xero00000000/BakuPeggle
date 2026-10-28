using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] bool playerArea;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerArea == true)
        {
            manager.currentBallsDestroyed += 1;
            if (manager.currentBallsDestroyed >= manager.maxBalls)
            {

                manager.ball1Destroyed = true;
            }
        }
        else
        {
            manager.ball2Destroyed = true;
        }
        Destroy(collision.gameObject);
    }
}
