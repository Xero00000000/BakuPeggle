using UnityEngine;

public class PersistentBullshit : MonoBehaviour
{
    public bool bakugan1;
    public bool bakugan2;

    private static PersistentBullshit instance;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
