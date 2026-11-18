using UnityEngine;

public class CinematicManager : MonoBehaviour
{
    [Header("Elementos")]
    public CinematicElement[] cinematicElements;

    void Start()
    {
        StartCinematic();
    }

    public void StartCinematic()
    {
        foreach (CinematicElement element in cinematicElements)
        {
            if (element != null)
            {
                element.PlayElement();
            }
        }
    }
}