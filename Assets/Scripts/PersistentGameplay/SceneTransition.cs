using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] float fadeDuration = 1f;

    private int fadeAmount = Shader.PropertyToID("FadeAmount");

    //private int useNombre = Shader.PropertyToID("nombre"); aca pones los shaders que estan dentro del coso
    private int useShader1 = Shader.PropertyToID("UseShader1");
    private int useShader2 = Shader.PropertyToID("UseShader2");
    private int useShader3 = Shader.PropertyToID("UseShader3");

    private int? lastEffect;

    private Image image;
    private Material material;
    private enum TransitionEffect
    {
        Shader1,
        Shader2,
        Shader3
        //[Obsolete("usa otro")] Shader4 //este es de ejemplo para acordarme a mi mismo si usamos esto en otro lado como hacer si borramos algo, porque se mueve todo y es un alboroto
    }

    [EnumButtons(true)]
    [SerializeField] TransitionEffect Effect;

    private void Awake()
    {
        image = GetComponent<Image>();

        Material mat = image.material;
        image.material = new Material(mat);
        material = image.material;

        lastEffect = useShader1;
    }

    private void FadeOut(TransitionEffect transitionType)
    {
        ChangeTransitionEffect(transitionType);
        StartFadeOut();
    }

    private void FadeIn(TransitionEffect transitionType)
    {
        ChangeTransitionEffect(transitionType);
        StartFadeIn();
    }

    private void ChangeTransitionEffect(TransitionEffect transitionType)
    {
        if (lastEffect.HasValue)
        {
            material.SetFloat(lastEffect.Value, 0f);
        }

        switch (transitionType)
        {
            case TransitionEffect.Shader1:
                SwitchEffect(useShader1);
                break;
            case TransitionEffect.Shader2:
                SwitchEffect(useShader2);
                break;
            case TransitionEffect.Shader3:
                SwitchEffect(useShader3);
                break;
        }
    }

    private void SwitchEffect(int effect)
    {
        material.SetFloat(effect, 1f);

        lastEffect = effect;
    }

    private void StartFadeOut()
    {
        material.SetFloat(fadeAmount, 0f);
        //material.DOFade(1f, fadeAmount, fadeDuration).SetEase(Ease, InOutSine); despues me fijo como hacer esto en primetween para no tener que hacer la corrutina

        StartCoroutine(HandleFade(1f, 0f));
    }

    private void StartFadeIn()
    {
        material.SetFloat(fadeAmount, 1f);

        StartCoroutine(HandleFade(0f, 1f));
    }

    private IEnumerator HandleFade(float targetAmount, float startAmount)
    {
        float elapsedTime = 0f;
        while(elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(startAmount, targetAmount, (elapsedTime / fadeDuration));

            yield return null;
        }

        material.SetFloat(fadeAmount, targetAmount);
    }

    //lo de aca es temporal para probar
    public void FadeInTest()
    {
        FadeIn(Effect);
    }

    public void FadeOutTest()
    {
        FadeOut(Effect);
    }
}
