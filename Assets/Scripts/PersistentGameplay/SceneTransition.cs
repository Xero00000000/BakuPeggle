using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;

    [Header("Materiales/Shaders (7 Elementos + 1 General)")]
    [SerializeField] private List<Material> transitionMaterials = new List<Material>();

<<<<<<< Updated upstream
    //private int useNombre = Shader.PropertyToID("nombre"); aca pones los shaders que estan dentro del coso
    private int useShader1 = Shader.PropertyToID("UseShader1");
    private int useShader2 = Shader.PropertyToID("UseShader2");
    private int useShader3 = Shader.PropertyToID("UseShader3");

    private int? lastEffect;
=======
    // Propiedad que se conecta con el Reference Name de Shader Graph
    private static readonly int FadeAmountProperty = Shader.PropertyToID("FadeAmount");
>>>>>>> Stashed changes

    private Image image;
    private Material currentMaterialInstance;
    private Coroutine currentFadeCoroutine;

    public enum TransitionEffect
    {
<<<<<<< Updated upstream
        Shader1,
        Shader2,
        Shader3
        //[Obsolete("usa otro")] Shader4 //este es de ejemplo para acordarme a mi mismo si usamos esto en otro lado como hacer si borramos algo, porque se mueve todo y es un alboroto
=======
        Agua,
        Fuego,
        Hielo,
        Tierra,
        Rayo,
        Neutro,
        Oscuro,
        ShaderTransicion
>>>>>>> Stashed changes
    }

    [SerializeField] private TransitionEffect effect;

    private void Awake()
    {
        image = GetComponent<Image>();
        ChangeTransitionEffect(effect);
    }

    private void OnDestroy()
    {
        if (currentMaterialInstance != null)
        {
            Destroy(currentMaterialInstance);
        }
    }

    public void FadeOut(TransitionEffect transitionType)
    {
        ChangeTransitionEffect(transitionType);
        StartFade(0f, 1f); 
    }

    public void FadeIn(TransitionEffect transitionType)
    {
        ChangeTransitionEffect(transitionType);
        StartFade(1f, 0f); // Transición (1 a 0)
    }

    private void ChangeTransitionEffect(TransitionEffect transitionType)
    {
        int materialIndex = (int)transitionType;

        if (transitionMaterials == null || materialIndex >= transitionMaterials.Count || transitionMaterials[materialIndex] == null)
        {
            Debug.LogError($"[SceneTransition] Falta asignar el material para {transitionType} en el índice {materialIndex} de la lista.");
            return;
        }

        if (currentMaterialInstance != null)
        {
<<<<<<< Updated upstream
            case TransitionEffect.Shader1:
                SwitchEffect(useShader1);
                break;
            case TransitionEffect.Shader2:
                SwitchEffect(useShader2);
                break;
            case TransitionEffect.Shader3:
                SwitchEffect(useShader3);
                break;
=======
            Destroy(currentMaterialInstance);
>>>>>>> Stashed changes
        }

        Material baseMaterial = transitionMaterials[materialIndex];
        currentMaterialInstance = new Material(baseMaterial);
        image.material = currentMaterialInstance;
    }

    private void StartFade(float startAmount, float targetAmount)
    {
        if (currentMaterialInstance == null) return;

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(HandleFade(startAmount, targetAmount));
    }

    private IEnumerator HandleFade(float startAmount, float targetAmount)
    {
        float elapsedTime = 0f;
        currentMaterialInstance.SetFloat(FadeAmountProperty, startAmount);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpedAmount = Mathf.Lerp(startAmount, targetAmount, elapsedTime / fadeDuration);

            if (currentMaterialInstance != null)
            {
                currentMaterialInstance.SetFloat(FadeAmountProperty, lerpedAmount);
            }

            yield return null;
        }

        if (currentMaterialInstance != null)
        {
            currentMaterialInstance.SetFloat(FadeAmountProperty, targetAmount);
        }

        currentFadeCoroutine = null;
    }
    public void FadeInTest()
    {
        FadeIn(effect);
    }

    public void FadeOutTest()
    {
        FadeOut(effect);
    }
}