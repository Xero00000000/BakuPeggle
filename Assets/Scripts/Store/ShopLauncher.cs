using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopLauncher : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private KeyCode openShopKey = KeyCode.T; 
    [SerializeField] private string shopSceneName = "ShopScene"; 

    void Update()
    {
        if (Input.GetKeyDown(openShopKey))
        {
            AbrirTienda();
        }
    }

    public void AbrirTienda()
    {
        SceneManager.LoadScene(shopSceneName);
    }
}