using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private PeggleUIManager uiManager;   // NEW: assign UIManager
    [SerializeField] bool playerArea;

    private void Awake()
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<PeggleUIManager>();
            Debug.LogWarning($"[DestroyArea] {name}: uiManager was empty, found it automatically: {(uiManager != null ? uiManager.name : "NOT FOUND")}", this);
        }
        if (manager == null)
        {
            manager = FindFirstObjectByType<TurnManager>();
            Debug.LogWarning($"[DestroyArea] {name}: manager was empty, found it automatically: {(manager != null ? manager.name : "NOT FOUND")}", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore a ball that's already shrinking so it can't be counted twice
        BallEffects fx = collision.GetComponentInParent<BallEffects>();
        if (fx != null && fx.IsVanishing) return;

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

        // Shrink + vanish VFX instead of an instant Destroy
        if (fx != null) fx.ShrinkAndVanish();
        else Destroy(collision.gameObject);
    }
}