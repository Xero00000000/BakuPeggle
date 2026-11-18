using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [Header("Opciones")]
    [Tooltip("Si está marcado, este trigger específico solo funcionará una vez.")]
    public bool usarUnaSolaVez = true;

    private bool yaSeUso = false;

    private void OnTriggerEnter(Collider other)
    {
        if (yaSeUso && usarUnaSolaVez) return;
        MenuCamera camaraMaestra = Camera.main.GetComponent<MenuCamera>();

        if (camaraMaestra != null)
        {
            camaraMaestra.AvanzarSiguienteParada();
            yaSeUso = true;
        }
        else
        {
            Debug.LogWarning("No se encontró el script MenuCamera en la Main Camera.", this);
        }
    }
}