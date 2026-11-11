using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private GameObject canvas;
    void Start()
    {
        canvas.SetActive(false);
    }

    public void aaa()
    {
        //StartCoroutine(timer());
        canvas.SetActive(true);
    }
    public IEnumerator timer()
    {
        yield return new WaitForSeconds(time);

    }

}
