using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Slider volumeSlider;

    [Header("Configuración de Retorno")]
    public string defaultReturnScene = "StreetScene";

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = Mathf.Clamp01(savedVolume);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0.0001f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = AudioListener.volume;

            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    public void GoBack()
    {
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(defaultReturnScene);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
        PlayerPrefs.Save();
    }
}