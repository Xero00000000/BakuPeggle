using UnityEngine;

public static class CameraLock
{
    private static MonoBehaviour owner;

    // Unity's null check: a destroyed object counts as null
    public static MonoBehaviour Owner => owner ? owner : null;

    public static bool TryAcquire(MonoBehaviour who)
    {
        if (!owner || owner == who) { owner = who; return true; }
        return false;
    }

    public static bool IsOwner(MonoBehaviour who) => owner && owner == who;

    public static void Release(MonoBehaviour who)
    {
        if (owner == who) owner = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() { owner = null; }
}