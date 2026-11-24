using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PhoneCombatApp : MonoBehaviour
{
    [Serializable]
    public struct AudioEntry
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Tooltip("0 = duración original del clip. > 0 = corta el audio a los X segundos")]
        [Range(0f, 5f)] public float maxDuration;
    }

    [Header("UI de la App del Teléfono")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [TextArea(3, 5)]
    [SerializeField] private string[] dialogueLines;

    [Header("Opciones de Combate")]
    [SerializeField] private GameObject combatChoicePanel;
    [SerializeField] private string combatSceneName = "Peggle 3";

    [Header("Audio de Diálogo")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioEntry[] voiceEntries;
    [SerializeField][Range(0.5f, 1.5f)] private float minPitch = 0.92f;
    [SerializeField][Range(0.5f, 1.5f)] private float maxPitch = 1.08f;
    [Range(0f, 1f)][SerializeField] private float blipInterval = 0f;
    [Range(0f, 3f)][SerializeField] private float blipDuration = 0.6f;

    [Header("Audio de Tensión al Pedir Combate")]
    [SerializeField] private AudioSource tensionAudioSource;
    [SerializeField] private AudioEntry tensionSound = new AudioEntry { volume = 0.7f, maxDuration = 0f };

    [Header("Audio de Decisión (Sí / No)")]
    [SerializeField] private AudioSource choiceAudioSource;
    [SerializeField] private AudioEntry acceptSound = new AudioEntry { volume = 1f, maxDuration = 0f };
    [SerializeField] private AudioEntry declineSound = new AudioEntry { volume = 1f, maxDuration = 0f };

    private Coroutine voiceBlipRoutine;
    private Coroutine voiceStopRoutine;
    private Coroutine tensionStopRoutine;
    private Coroutine choiceStopRoutine;

    private int currentLineIndex = 0;
    private bool dialogueEnded = false;
    private bool blockClickThisFrame = false;

    private enum AudioChannel { Voice, Tension, Choice }

    void Awake()
    {
        if (voiceAudioSource == null) voiceAudioSource = GetComponent<AudioSource>();
        if (tensionAudioSource == null) tensionAudioSource = voiceAudioSource;
        if (choiceAudioSource == null) choiceAudioSource = voiceAudioSource;
    }

    void Start()
    {
        ResetAppState();
    }

    void Update()
    {
        if (blockClickThisFrame)
        {
            blockClickThisFrame = false;
            return;
        }

        if (dialoguePanel != null && dialoguePanel.activeInHierarchy && !dialogueEnded)
        {
            if (Input.GetMouseButtonDown(0))
            {
                NextLine();
            }
        }
    }

    // Asignar al botón/ícono que abre la app o contacto en el teléfono
    public void OpenCombatDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        StopTensionSound();
        dialogueEnded = false;
        currentLineIndex = 0;
        blockClickThisFrame = true;

        if (combatChoicePanel != null) combatChoicePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);

        ShowCurrentLine();
    }

    public void NextLine()
    {
        if (dialogueEnded) return;

        StopLineVoice();
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogueAndAskForCombat();
        }
    }

    private void ShowCurrentLine()
    {
        if (dialogueText != null && currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }

        PlayLineVoice();
    }

    private void EndDialogueAndAskForCombat()
    {
        StopLineVoice();
        dialogueEnded = true;
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (combatChoicePanel != null) combatChoicePanel.SetActive(true);

        PlayTensionSound();
    }

    // Asignar al botón "Sí"
    public void AcceptCombat()
    {
        StopLineVoice();
        StopTensionSound();
        PlayChoiceSound(acceptSound);

        if (combatChoicePanel != null) combatChoicePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (!string.IsNullOrEmpty(combatSceneName))
        {
            SceneManager.LoadScene(combatSceneName);
        }
    }

    // Asignar al botón "No"
    public void DeclineCombat()
    {
        StopLineVoice();
        StopTensionSound();
        PlayChoiceSound(declineSound);

        ResetAppState();
    }

    public void ResetAppState()
    {
        StopLineVoice();
        StopTensionSound();
        dialogueEnded = false;
        currentLineIndex = 0;

        if (combatChoicePanel != null) combatChoicePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
    }

    private void PlayLineVoice()
    {
        if (voiceAudioSource == null || voiceEntries == null || voiceEntries.Length == 0) return;

        StopLineVoice();

        if (blipInterval > 0f)
            voiceBlipRoutine = StartCoroutine(PlayRepeatedBlips());
        else
            TriggerSingleBlip();
    }

    private IEnumerator PlayRepeatedBlips()
    {
        float elapsed = 0f;
        while (elapsed < blipDuration && !dialogueEnded)
        {
            TriggerSingleBlip();
            yield return new WaitForSeconds(blipInterval);
            elapsed += blipInterval;
        }
        voiceBlipRoutine = null;
    }

    private void TriggerSingleBlip()
    {
        AudioEntry entry = voiceEntries[UnityEngine.Random.Range(0, voiceEntries.Length)];
        if (entry.clip == null || voiceAudioSource == null) return;

        if (voiceStopRoutine != null)
        {
            StopCoroutine(voiceStopRoutine);
            voiceStopRoutine = null;
        }

        voiceAudioSource.Stop();
        voiceAudioSource.clip = entry.clip;
        voiceAudioSource.volume = Mathf.Clamp01(entry.volume);
        voiceAudioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        voiceAudioSource.Play();

        if (entry.maxDuration > 0f)
            voiceStopRoutine = StartCoroutine(StopSourceAfter(voiceAudioSource, entry.maxDuration, AudioChannel.Voice));
    }

    private void StopLineVoice()
    {
        if (voiceBlipRoutine != null)
        {
            StopCoroutine(voiceBlipRoutine);
            voiceBlipRoutine = null;
        }

        if (voiceStopRoutine != null)
        {
            StopCoroutine(voiceStopRoutine);
            voiceStopRoutine = null;
        }

        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
            voiceAudioSource.clip = null;
            voiceAudioSource.pitch = 1f;
        }
    }

    private void PlayTensionSound()
    {
        if (tensionSound.clip == null || tensionAudioSource == null) return;

        StopTensionSound();

        tensionAudioSource.clip = tensionSound.clip;
        tensionAudioSource.volume = Mathf.Clamp01(tensionSound.volume);
        tensionAudioSource.pitch = 1f;
        tensionAudioSource.Play();

        if (tensionSound.maxDuration > 0f)
            tensionStopRoutine = StartCoroutine(StopSourceAfter(tensionAudioSource, tensionSound.maxDuration, AudioChannel.Tension));
    }

    private void StopTensionSound()
    {
        if (tensionStopRoutine != null)
        {
            StopCoroutine(tensionStopRoutine);
            tensionStopRoutine = null;
        }

        if (tensionAudioSource != null)
        {
            tensionAudioSource.Stop();
            tensionAudioSource.clip = null;
            tensionAudioSource.pitch = 1f;
        }
    }

    private void PlayChoiceSound(AudioEntry entry)
    {
        if (entry.clip == null || choiceAudioSource == null) return;

        if (choiceStopRoutine != null)
        {
            StopCoroutine(choiceStopRoutine);
            choiceStopRoutine = null;
        }

        choiceAudioSource.Stop();
        choiceAudioSource.clip = entry.clip;
        choiceAudioSource.volume = Mathf.Clamp01(entry.volume);
        choiceAudioSource.pitch = 1f;
        choiceAudioSource.Play();

        if (entry.maxDuration > 0f)
            choiceStopRoutine = StartCoroutine(StopSourceAfter(choiceAudioSource, entry.maxDuration, AudioChannel.Choice));
    }

    private IEnumerator StopSourceAfter(AudioSource source, float duration, AudioChannel channel)
    {
        yield return new WaitForSeconds(duration);

        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }

        if (channel == AudioChannel.Voice) voiceStopRoutine = null;
        else if (channel == AudioChannel.Tension) tensionStopRoutine = null;
        else choiceStopRoutine = null;
    }
}