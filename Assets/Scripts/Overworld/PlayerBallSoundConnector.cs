using System.Collections;
using UnityEngine;

public class PlayerBallSoundConnector : MonoBehaviour
{
    [SerializeField] private PegLauncher launcher;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float matchRadius = 0.75f;
    [SerializeField] private float watchDuration = 0.35f;

    private Coroutine watchRoutine;

    void Awake()
    {
        if (launcher == null)
        {
            launcher = GetComponent<PegLauncher>();
        }
    }

    void Update()
    {
        if (launcher == null) return;

        if (Input.GetMouseButtonDown(0) && launcher.IsPlayerTurn)
        {
            if (watchRoutine != null)
            {
                StopCoroutine(watchRoutine);
            }
            watchRoutine = StartCoroutine(TagSpawnedPlayerBalls());
        }
    }

    private IEnumerator TagSpawnedPlayerBalls()
    {
        float timer = 0f;
        Vector3 origin = spawnPoint != null ? spawnPoint.position : launcher.transform.position;

        while (timer < watchDuration)
        {
            BallEffects[] allBalls = FindObjectsByType<BallEffects>(FindObjectsSortMode.None);
            for (int i = 0; i < allBalls.Length; i++)
            {
                BallEffects ball = allBalls[i];
                if (ball == null) continue;

                if (Vector2.Distance(ball.transform.position, origin) <= matchRadius)
                {
                    ball.SetOnlyWhenPlayerBall(true);
                }
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        watchRoutine = null;
    }
}