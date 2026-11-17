using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToStreet : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Escribe exactamente el nombre de la escena de la calle.")]
    [SerializeField] private string streetSceneName = "Calle"; 

    public void ReturnToStreet()
    {
        SceneManager.LoadScene(streetSceneName);
    }
}