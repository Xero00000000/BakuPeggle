using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToStreet : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Escribe exactamente el nombre de la escena de la calle.")]
    [SerializeField] private string streetSceneName = "Calle"; // Reemplaza "Calle" por el nombre real de tu escena

    public void ReturnToStreet()
    {
        SceneManager.LoadScene(streetSceneName);
    }
}