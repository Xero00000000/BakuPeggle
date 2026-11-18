using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    [Header("Secuencia Global de Paradas de Cámara")]
    [Tooltip("Arrastra aquí todos los Transforms (puntos) de la escena en el orden exacto que quieres visitar.")]
    public Transform[] paradasDeCamara;

    [Header("Configuración de Transición")]
    [Tooltip("Velocidad con la que la cámara viaja de una parada a otra.")]
    public float velocidadTransicion = 3f;

    [Tooltip("Si está marcado, al llegar al final de la lista volverá a empezar desde la primera parada.")]
    public bool repetirEnBucle = true;
    private int indiceActual = -1;
    private Transform destinoActual = null;
    private bool enTransicion = false;

    void LateUpdate()
    {
        if (enTransicion && destinoActual != null)
        {
            transform.position = Vector3.Lerp(transform.position, destinoActual.position, velocidadTransicion * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, destinoActual.rotation, velocidadTransicion * Time.deltaTime);
        }
    }

    public void AvanzarSiguienteParada()
    {
        if (paradasDeCamara == null || paradasDeCamara.Length == 0)
        {
            Debug.LogWarning("La lista de paradas de cámara está vacía en la cámara.", this);
            return;
        }

        indiceActual++;
        if (indiceActual >= paradasDeCamara.Length)
        {
            if (repetirEnBucle)
            {
                indiceActual = 0; 
            }
            else
            {
                indiceActual = paradasDeCamara.Length - 1; 
            }
        }

        destinoActual = paradasDeCamara[indiceActual];
        enTransicion = true;

        Debug.Log("Cámara avanzando a la parada: " + destinoActual.name + " (Índice: " + indiceActual + ")");
    }
}