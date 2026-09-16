using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RestoreAberration : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    [SerializeField] private bool filterByTag = false;
    [SerializeField] private string targetTag = "Player";

    private ChromaticAberration chromaticComponent;

    private void Start()
    {
        SetupVolumeReference();
    }

    private void SetupVolumeReference()
    {
        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<Volume>();
        }

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticComponent);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryResetChromatic(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        TryResetChromatic(collider.gameObject);
    }

    private void TryResetChromatic(GameObject collidedObject)
    {
        if (filterByTag && !collidedObject.CompareTag(targetTag))
        {
            return;
        }

        ResetChromaticAberration();
    }

    public void ResetChromaticAberration()
    {
        Volume activeVolume = FindFirstObjectByType<Volume>();

        if (activeVolume != null && activeVolume.profile != null)
        {
            if (activeVolume.profile.TryGet(out ChromaticAberration chromatic))
            {
                chromatic.intensity.overrideState = true;
                chromatic.intensity.value = 0f;
            }
        }
    }
}