using System.Collections;
using TMPro;
using UnityEngine;

public class ShopAudioAndDialogueManager : MonoBehaviour
{
    public static ShopAudioAndDialogueManager Instance;

    [Header("Música de la Tienda")]
    public AudioSource bgmSource;
    public AudioLowPassFilter lowPassFilter;

    [Header("Valores de Volumen")]
    [Range(0f, 1f)] public float streetVolume = 0.2f;
    [Range(0f, 1f)] public float approachVolume = 0.5f;
    [Range(0f, 1f)] public float insideVolume = 0.9f;
    public float fadeSpeed = 2.5f;

    [Header("Valores del Filtro de Pared (Hz)")]
    public float streetCutoff = 1200f;
    public float approachCutoff = 3500f;
    public float insideCutoff = 22000f;

    [Header("Efectos de Tienda")]
    public AudioSource sfxSource;
    public AudioClip doorChimeClip;

    [Header("Sistema de Diálogos")]
    public AudioSource voiceSource;
    public AudioClip voiceBlipClip;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    [Header("Timbre y Personalización de Voz")]
    [Range(0.2f, 3f)]
    [Tooltip("Tono base del cajero: < 1 voz grave/ronca, 1 normal, > 1 aguda/animada")]
    public float voiceBasePitch = 1.0f;

    [Range(0f, 0.6f)]
    [Tooltip("Variación tonal aleatoria entre letras (0 = robot monótono)")]
    public float voicePitchVariation = 0.08f;

    [Range(0f, 1f)]
    [Tooltip("Volumen individual de los balbuceos")]
    public float voiceVolume = 0.85f;

    [Range(1, 5)]
    [Tooltip("Cada cuántos caracteres suena el balbuceo (1 = cada letra, 2 o 3 = estilo Animal Crossing/Undertale menos saturado)")]
    public int blipCharacterFrequency = 1;

    [TextArea(2, 4)]
    public string[] dialogues = new string[]
    {
        "¡Bienvenido! ¿Buscás algo especial hoy?",
        "Tengo las mejores mejoras para tu lanzador. Echá un vistazo."
    };

    public float textSpeed = 0.04f;
    private int currentDialogueIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private float targetVolume;
    private float targetCutoff;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (lowPassFilter == null)
            lowPassFilter = GetComponent<AudioLowPassFilter>();
    }

    void Start()
    {
        targetVolume = streetVolume;
        targetCutoff = streetCutoff;

        if (bgmSource != null)
        {
            bgmSource.volume = streetVolume;
            if (!bgmSource.isPlaying) bgmSource.Play();
        }

        if (lowPassFilter != null)
        {
            lowPassFilter.cutoffFrequency = streetCutoff;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (bgmSource != null && Mathf.Abs(bgmSource.volume - targetVolume) > 0.01f)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, targetVolume, Time.deltaTime * fadeSpeed);
        }

        if (lowPassFilter != null && Mathf.Abs(lowPassFilter.cutoffFrequency - targetCutoff) > 10f)
        {
            lowPassFilter.cutoffFrequency = Mathf.MoveTowards(lowPassFilter.cutoffFrequency, targetCutoff, Time.deltaTime * fadeSpeed * 5000f);
        }
    }

    public void SetStreetState()
    {
        targetVolume = streetVolume;
        targetCutoff = streetCutoff;
    }

    public void SetApproachState()
    {
        targetVolume = approachVolume;
        targetCutoff = approachCutoff;
    }

    public void EnterShopState()
    {
        targetVolume = insideVolume;
        targetCutoff = insideCutoff;

        if (sfxSource != null && doorChimeClip != null)
        {
            sfxSource.PlayOneShot(doorChimeClip);
        }

        StartDialogue();
    }

    public void StartDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        currentDialogueIndex = 0;
        DisplayCurrentSentence();
    }

    public void NextDialogue()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            StopVoice();
            if (dialogueText != null) dialogueText.text = dialogues[currentDialogueIndex];
            isTyping = false;
            return;
        }

        currentDialogueIndex++;
        if (currentDialogueIndex < dialogues.Length)
        {
            DisplayCurrentSentence();
        }
        else
        {
            StopVoice();
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }
    }

    private void DisplayCurrentSentence()
    {
        if (dialogueText == null) return;
        StopVoice();
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(dialogues[currentDialogueIndex]));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        int charCounter = 0;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            if (!char.IsWhiteSpace(letter))
            {
                charCounter++;
                if (charCounter % blipCharacterFrequency == 0)
                {
                    PlayVoiceBlip();
                }
            }

            yield return new WaitForSeconds(textSpeed);
        }

        StopVoice();
        isTyping = false;
        typingCoroutine = null;
    }

    private void PlayVoiceBlip()
    {
        if (voiceSource == null || voiceBlipClip == null) return;

        float randomPitch = Random.Range(-voicePitchVariation, voicePitchVariation);
        voiceSource.pitch = Mathf.Clamp(voiceBasePitch + randomPitch, 0.1f, 3f);
        voiceSource.volume = voiceVolume;

        voiceSource.PlayOneShot(voiceBlipClip);
    }

    private void StopVoice()
    {
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }
    }
}