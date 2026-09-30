using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip soundOn;
    [SerializeField] private AudioClip soundOff;

    [Header("Configuración")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;


    public void PlaySoundOn()
    {
        PlayClip(soundOn);
    }
    public void PlaySoundOff()
    {
        PlayClip(soundOff);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }
        else
        {
            Debug.LogWarning("AudioClip no asignado en el componente PlaySoundOnClick.", this);
        }
    }
}