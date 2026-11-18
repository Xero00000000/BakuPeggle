using UnityEngine;
using UnityEngine.Rendering.Universal; // Necesario para acceder a URP

public class ControlLight : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El componente de datos adicionales de URP (búscalo automáticamente si se deja vacío).")]
    public UniversalAdditionalLightData lightData;

    [Tooltip("Cuánto aumenta el Cookie Size en Y por cada segundo transcurrido.")]
    public float velocidadDeAumento = 2f;

    [Tooltip("Valor máximo que puede alcanzar en Y (opcional).")]
    public float tamañoMaximoY = 50f;

    void Start()
    {
        // Si no se asignó en el inspector, lo busca en este mismo objeto
        if (lightData == null)
        {
            lightData = GetComponent<UniversalAdditionalLightData>();
        }
    }

    void Update()
    {
        if (lightData != null)
        {
            // Obtenemos el Vector2 actual del Cookie Size de URP
            Vector2 tamañoActual = lightData.lightCookieSize;

            // Aumentamos únicamente el valor de Y de forma gradual
            tamañoActual.y += velocidadDeAumento * Time.deltaTime;

            // Aplicamos un límite máximo opcional
            if (tamañoMaximoY > 0f && tamañoActual.y >= tamañoMaximoY)
            {
                tamañoActual.y = tamañoMaximoY;
            }

            // Guardamos el nuevo valor de vuelta en URP
            lightData.lightCookieSize = tamañoActual;
        }
    }
}