using UnityEngine;

public class Outliner : MonoBehaviour
{
    [SerializeField] private GameObject Outline;

    private void OnMouseOver()
    {
        Outline.SetActive(true);
    }
    private void OnMouseExit()
    {
        Outline.SetActive(false);
    }
}
