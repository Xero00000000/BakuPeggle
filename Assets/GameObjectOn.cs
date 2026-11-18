using UnityEngine;

public class GameObjectOn : MonoBehaviour
{
    public GameObject objetoParaPrender;
    public GameObject objetoParaPrender2;
    private void OnCollisionEnter(Collision collision)
    {
        if (objetoParaPrender != null)
        {
            objetoParaPrender.SetActive(true);
        }
        if (objetoParaPrender2 != null)
        {
            objetoParaPrender2.SetActive(true);
        }
    }
}
