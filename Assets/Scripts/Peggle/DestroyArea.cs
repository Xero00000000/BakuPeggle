using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private PeggleUIManager uiManager;   // NEW: assign UIManager
    [SerializeField] bool playerArea;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector3 landPos = collision.transform.position;

        if (playerArea == true)
        {
            manager.currentBallsDestroyed += 1;
            if (manager.currentBallsDestroyed >= manager.maxBalls)
            {
                manager.ball1Destroyed = true;
                if (uiManager != null) uiManager.ShowTotalDamage(true, landPos);
            }
        }
        else
        {
            manager.ball2Destroyed = true;
            if (uiManager != null) uiManager.ShowTotalDamage(false, landPos);
        }
        Destroy(collision.gameObject);
    }
}