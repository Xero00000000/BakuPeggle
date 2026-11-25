using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    [Serializable]
    public struct AudioEntry
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Tooltip("0 = duración original del clip. > 0 = corta el audio a los X segundos")]
        [Range(0f, 5f)] public float maxDuration;
    }

    [Header("UI de Diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [TextArea(3, 5)]
    [SerializeField] private string[] dialogueLines;

    [Header("Audio de Diálogo")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioEntry[] voiceEntries;
    [SerializeField][Range(0.5f, 1.5f)] private float minPitch = 0.92f;
    [SerializeField][Range(0.5f, 1.5f)] private float maxPitch = 1.08f;
    [Tooltip("0 = suena una vez al aparecer la línea. > 0 = repite cada X segundos mientras habla")]
    [Range(0f, 1f)][SerializeField] private float blipInterval = 0f;
    [Range(0f, 3f)][SerializeField] private float blipDuration = 0.6f;

    [Header("Audio de Tensión al Pedir Combate")]
    [SerializeField] private AudioSource tensionAudioSource;
    [SerializeField] private AudioEntry tensionSound = new AudioEntry { volume = 0.7f, maxDuration = 0f };

    [Header("Audio de Decisión (Sí / No)")]
    [SerializeField] private AudioSource choiceAudioSource;
    [SerializeField] private AudioEntry acceptSound = new AudioEntry { volume = 1f, maxDuration = 0f };
    [SerializeField] private AudioEntry declineSound = new AudioEntry { volume = 1f, maxDuration = 0f };

    [Header("Opciones de Combate")]
    [SerializeField] private GameObject combatChoicePanel;
    [SerializeField] private string combatSceneName = "Peggle 3";

    [Header("Sincronización con CameraMove")]
    [SerializeField] private bool waitForCameraClose = true;
    [SerializeField] private float closeThreshold = 0.6f;

    private CameraMove cameraMove;
    private Camera targetCamera;
    private FieldInfo objPosField;
    private FieldInfo isCloseField;
    private FieldInfo loadField;
    private MethodInfo loadNextSceneMethod;

    private Coroutine voiceBlipRoutine;
    private Coroutine voiceStopRoutine;
    private Coroutine tensionStopRoutine;
    private Coroutine choiceStopRoutine;

    private int currentLineIndex = 0;
    private bool dialogueEnded = false;
    private bool dialogueStarted = false;
    private bool blockClickThisFrame = false;

    void Awake()
    {
        cameraMove = GetComponent<CameraMove>();
        targetCamera = (cameraMove != null && cameraMove._camera != null) ? cameraMove._camera : Camera.main;

        if (voiceAudioSource == null)
        {
            voiceAudioSource = GetComponent<AudioSource>();
        }

        if (tensionAudioSource == null)
        {
            tensionAudioSource = voiceAudioSource;
        }

        if (choiceAudioSource == null)
        {
            choiceAudioSource = voiceAudioSource;
        }

        if (cameraMove != null)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            objPosField = typeof(CameraMove).GetField("objPos", flags);
            isCloseField = typeof(CameraMove).GetField("isClose", flags);
            loadField = typeof(CameraMove).GetField("load", flags);
            loadNextSceneMethod = typeof(CameraMove).GetMethod("LoadNextScene", flags);
        }
    }

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (combatChoicePanel != null) combatChoicePanel.SetActive(false);
    }

    void Update()
    {
        if (waitForCameraClose && cameraMove != null && cameraMove.enabled && !dialogueStarted && targetCamera != null)
        {
            bool ownsCamera = CameraLock.IsOwner(cameraMove);
            bool isClose = isCloseField != null && (bool)isCloseField.GetValue(cameraMove);

            if (ownsCamera && isClose)
            {
                if (loadField != null) loadField.SetValue(cameraMove, false);
                cameraMove.CancelInvoke("SceneLoader");

                Vector3 targetPos = objPosField != null
                    ? (Vector3)objPosField.GetValue(cameraMove)
                    : transform.position;

                float dist = Vector3.Distance(targetCamera.transform.position, targetPos);
                if (dist <= closeThreshold)
                {
                    StartDialogue();
                }
            }
        }

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

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0 || dialogueStarted) return;

        StopTensionSound();
        dialogueStarted = true;
        dialogueEnded = false;
        currentLineIndex = 0;
        blockClickThisFrame = true;

        if (cameraMove != null)
        {
            if (loadField != null) loadField.SetValue(cameraMove, false);
            cameraMove.CancelInvoke("SceneLoader");
            cameraMove.enabled = false;
        }

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
        if (dialogueText != null && dialogueLines != null && currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }

        PlayLineVoice();
    }

    private void PlayLineVoice()
    {
        if (voiceAudioSource == null || voiceEntries == null || voiceEntries.Length == 0) return;

        StopLineVoice();

        if (blipInterval > 0f)
        {
            voiceBlipRoutine = StartCoroutine(PlayRepeatedBlips());
        }
        else
        {
            TriggerSingleBlip();
        }
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
        {
            voiceStopRoutine = StartCoroutine(StopSourceAfter(voiceAudioSource, entry.maxDuration, AudioChannel.Voice));
        }
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
        {
            tensionStopRoutine = StartCoroutine(StopSourceAfter(tensionAudioSource, tensionSound.maxDuration, AudioChannel.Tension));
        }
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
        {
            choiceStopRoutine = StartCoroutine(StopSourceAfter(choiceAudioSource, entry.maxDuration, AudioChannel.Choice));
        }
    }

    private enum AudioChannel
    {
        Voice,
        Tension,
        Choice
    }

    private IEnumerator StopSourceAfter(AudioSource source, float duration, AudioChannel channel)
    {
        yield return new WaitForSeconds(duration);

        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }

        if (channel == AudioChannel.Voice)
            voiceStopRoutine = null;
        else if (channel == AudioChannel.Tension)
            tensionStopRoutine = null;
        else
            choiceStopRoutine = null;
    }

    private void EndDialogueAndAskForCombat()
    {
        StopLineVoice();
        dialogueEnded = true;
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (combatChoicePanel != null) combatChoicePanel.SetActive(true);

        PlayTensionSound();
    }

    public void AcceptCombat()
    {
        StopLineVoice();
        StopTensionSound();
        PlayChoiceSound(acceptSound);

        if (combatChoicePanel != null) combatChoicePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (cameraMove != null && loadNextSceneMethod != null)
        {
            loadNextSceneMethod.Invoke(cameraMove, null);
            return;
        }

        string targetScene = (cameraMove != null && !string.IsNullOrEmpty(cameraMove.sceneName))
            ? cameraMove.sceneName
            : combatSceneName;

        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    public void DeclineCombat()
    {
        StopLineVoice();
        StopTensionSound();
        PlayChoiceSound(declineSound);

        if (combatChoicePanel != null) combatChoicePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);

        dialogueEnded = false;
        dialogueStarted = false;
        currentLineIndex = 0;

        if (cameraMove != null)
        {
            if (isCloseField != null) isCloseField.SetValue(cameraMove, false);
            if (loadField != null) loadField.SetValue(cameraMove, false);
            cameraMove.CancelInvoke("SceneLoader");
            cameraMove.enabled = true;
        }
    }
}