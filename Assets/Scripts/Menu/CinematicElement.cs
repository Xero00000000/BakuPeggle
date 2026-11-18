using System.Collections;
using UnityEngine;

public class CinematicElement : MonoBehaviour
{
    [Header("Puntos")]
    public Transform[] waypoints;
    public float moveTimeTotal = 3f;
    public float delayInicio = 0f;

    [Header("Rotar")]
    public bool rotarEnX = true;
    public bool rotarEnY = true;
    public bool rotarEnZ = true;

    [Header("Active")]
    public GameObject[] setOnGameobject;
    public GameObject[] setOffGameobject;

    [Header("Deformar")]
    public float deformIntensity = 0.3f;

    [Header("Audio")]
    public AudioClip[] waypointsAudios;
    private AudioSource audioSource;

    [Header("Variables Inicio")]
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && waypointsAudios != null && waypointsAudios.Length > 0)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayElement()
    {
        initialRotation = transform.rotation;
        StartCoroutine(CinematicRoutine());
    }

    private IEnumerator CinematicRoutine()
    {
        if (delayInicio > 0f)
        {
            yield return new WaitForSeconds(delayInicio);
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            yield break;
        }

        if (waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
            ApplyMaskedRotation(waypoints[0].rotation);
            PlayAudioForWaypoint(0);
        }

        ActiveGameobjects(true);

        if (waypoints.Length == 1) yield break;

        int totalSegmentos = waypoints.Length - 1;
        float tiempoPorSegmento = moveTimeTotal / totalSegmentos;

        for (int i = 0; i < totalSegmentos; i++)
        {
            Transform puntoActual = waypoints[i];
            Transform puntoSiguiente = waypoints[i + 1];

            if (puntoActual == null || puntoSiguiente == null) continue;

            float time = 0f;
            while (time < tiempoPorSegmento)
            {
                time += Time.deltaTime;
                float progress = time / tiempoPorSegmento;
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                transform.position = Vector3.Lerp(puntoActual.position, puntoSiguiente.position, smoothProgress);

                Quaternion targetRot = Quaternion.Slerp(puntoActual.rotation, puntoSiguiente.rotation, smoothProgress);
                ApplyMaskedRotation(targetRot);

                float deformation = Mathf.Sin(progress * Mathf.PI) * deformIntensity;
                transform.localScale = new Vector3(
                    initialScale.x + deformation,
                    initialScale.y - deformation,
                    initialScale.x + deformation
                );

                yield return null;
            }

            transform.position = puntoSiguiente.position;
            ApplyMaskedRotation(puntoSiguiente.rotation);
            transform.localScale = initialScale;

            PlayAudioForWaypoint(i + 1);
        }
    }

    private void PlayAudioForWaypoint(int index)
    {
        if (audioSource != null && waypointsAudios != null && index < waypointsAudios.Length)
        {
            if (waypointsAudios[index] != null)
            {
                audioSource.PlayOneShot(waypointsAudios[index]);
            }
        }
    }

    private void ApplyMaskedRotation(Quaternion targetRot)
    {
        Vector3 targetEuler = targetRot.eulerAngles;
        Vector3 initialEuler = initialRotation.eulerAngles;

        float rx = rotarEnX ? targetEuler.x : initialEuler.x;
        float ry = rotarEnY ? targetEuler.y : initialEuler.y;
        float rz = rotarEnZ ? targetEuler.z : initialEuler.z;

        transform.rotation = Quaternion.Euler(rx, ry, rz);
    }

    private void ActiveGameobjects(bool on)
    {
        if (setOnGameobject != null)
        {
            foreach (GameObject obj in setOnGameobject)
            {
                if (obj != null) obj.SetActive(on);
            }
        }

        if (setOffGameobject != null)
        {
            foreach (GameObject obj in setOffGameobject)
            {
                if (obj != null) obj.SetActive(!on);
            }
        }
    }
}