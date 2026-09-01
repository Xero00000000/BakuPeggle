using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("Referencias UI")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text bodyText;

    [Header("Efecto de Escritura")]
    public float typingSpeed = 0.03f;

    private Queue<string> sentences = new Queue<string>();
    private bool isTyping = false;
    private string currentSentence = "";
    private Action onDialogueComplete;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    public void StartDialogue(string speakerName, string[] lines, Action callbackOnFinish = null)
    {
        onDialogueComplete = callbackOnFinish;
        panel.SetActive(true);
        nameText.text = speakerName;

        sentences.Clear();
        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    void Update()
    {
        bool clickTriggered = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool spaceTriggered = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (panel.activeSelf && (clickTriggered || spaceTriggered))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                bodyText.text = currentSentence;
                isTyping = false;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    private void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        bodyText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            bodyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        panel.SetActive(false);
        onDialogueComplete?.Invoke();
    }
}