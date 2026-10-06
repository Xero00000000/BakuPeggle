using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PhoneRotator : MonoBehaviour
{
    [Header("Contenedor Principal")]
    [Tooltip("El contenedor completo del telefono (PhoneFull)")]
    [SerializeField] private RectTransform phoneFullTransform;

    [Header("Contenedor o Textos a Ocultar")]
    [Tooltip("Arrastra AppsContainer para que busque los textos automaticamente")]
    [SerializeField] private GameObject appsContainer;

    [Tooltip("Opcional: Si queres asignar textos especificos manualmente")]
    [SerializeField] private List<GameObject> textsToHide = new List<GameObject>();

    [Header("Ajustes de Posicion en Horizontal")]
    [Tooltip("Desplazamiento a la izquierda para que entre todo el telefono en pantalla")]
    [SerializeField] private float landscapeOffsetX = -350f;

    [Tooltip("Desplazamiento vertical para centrar")]
    [SerializeField] private float landscapeOffsetY = 180f;

    [Header("Configuracion de Giro")]
    [SerializeField] private KeyCode toggleKey = KeyCode.G;
    [SerializeField] private float rotationDuration = 0.3f;

    private bool isLandscape = false;
    private Coroutine currentCoroutine;
    private Vector2 defaultPhoneAnchoredPos;
    private List<TextMeshProUGUI> cachedTMPTexts = new List<TextMeshProUGUI>();

    void Awake()
    {
        if (phoneFullTransform != null)
        {
            defaultPhoneAnchoredPos = phoneFullTransform.anchoredPosition;
        }

        if (appsContainer != null)
        {
            cachedTMPTexts.AddRange(appsContainer.GetComponentsInChildren<TextMeshProUGUI>(true));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (phoneFullTransform != null && phoneFullTransform.gameObject.activeInHierarchy)
            {
                ToggleOrientation();
            }
        }
    }

    public void ToggleOrientation()
    {
        isLandscape = !isLandscape;

        float targetPhoneZ = isLandscape ? 90f : 0f;
        Vector2 targetPhonePos = isLandscape
            ? defaultPhoneAnchoredPos + new Vector2(landscapeOffsetX, landscapeOffsetY)
            : defaultPhoneAnchoredPos;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(AnimateOrientation(targetPhoneZ, targetPhonePos, isLandscape));
    }

    private IEnumerator AnimateOrientation(float targetPhoneZ, Vector2 targetPhonePos, bool landscape)
    {
        if (landscape)
        {
            SetTextsVisibility(false);
        }

        float elapsed = 0f;
        Quaternion initPhoneRot = phoneFullTransform.localRotation;
        Quaternion targetPhoneRot = Quaternion.Euler(0f, 0f, targetPhoneZ);
        Vector2 initPhonePos = phoneFullTransform.anchoredPosition;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);

            phoneFullTransform.localRotation = Quaternion.Slerp(initPhoneRot, targetPhoneRot, t);
            phoneFullTransform.anchoredPosition = Vector2.Lerp(initPhonePos, targetPhonePos, t);

            yield return null;
        }

        phoneFullTransform.localRotation = targetPhoneRot;
        phoneFullTransform.anchoredPosition = targetPhonePos;

        if (!landscape)
        {
            SetTextsVisibility(true);
        }

        currentCoroutine = null;
    }

    private void SetTextsVisibility(bool visible)
    {
        for (int i = 0; i < cachedTMPTexts.Count; i++)
        {
            if (cachedTMPTexts[i] != null)
            {
                cachedTMPTexts[i].enabled = visible;
            }
        }

        for (int i = 0; i < textsToHide.Count; i++)
        {
            if (textsToHide[i] != null)
            {
                textsToHide[i].SetActive(visible);
            }
        }
    }
}