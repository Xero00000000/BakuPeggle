using UnityEngine;

public class PegLauncher : MonoBehaviour
{
    public Vector3 mousePos;
    [SerializeField] float minAngle;
    [SerializeField] float maxAngle;
    [SerializeField] float snapAngle;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float force;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;

    //temporal hasta que mejore los turnos
    private bool isTurn;
    [SerializeField] private ShotEventChannel _shoot;

    void Update()
    {
        /*
        Vector3 direction = new Vector3(mousePos.x, mousePos.z, mousePos.y) - transform.position;
        //direction.y = 0f;


        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0f, 0f, lookRotation.eulerAngles.y);
        }*/

        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //float snappedAngle = Mathf.Round(angle / snapAngle) * snapAngle;
        float clampedAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0, 0, clampedAngle);

        if (Input.GetMouseButtonDown(0) && isTurn == true)
        {
            audioSource.PlayOneShot(audioClip, 1f);
            var instance = Instantiate(ballPrefab, spawnPoint.transform.position, Quaternion.identity);
            instance.GetComponent<Rigidbody2D>().AddForce((direction) * force);
            _shoot.Raise(this, 1);
            isTurn = false;
        }
    }

    public void NewTurn(Component that)
    {
        isTurn = true;
    }
}
