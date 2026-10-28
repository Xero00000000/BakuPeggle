using UnityEngine;

public class PegLauncher : MonoBehaviour
{
    public Vector3 mousePos;
    [SerializeField] float minAngle;
    [SerializeField] float maxAngle;
    [SerializeField] float snapAngle;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject otherBallPrefab;
    [SerializeField] private GameObject cloneBallPrefab;
    [SerializeField] private GameObject bombBallPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float force;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;

    //temporal hasta que mejore los turnos
    [SerializeField] private bool isTurn;
    [SerializeField] private ShotEventChannel _shoot;

    private GameObject currentBall;

    private void Start()
    {
        isTurn = true;
        currentBall = ballPrefab;
    }

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
            var instance = Instantiate(currentBall, spawnPoint.transform.position, Quaternion.identity);
            instance.GetComponent<Rigidbody2D>().AddForce((direction) * force);
            _shoot.Raise(this, 1);
            isTurn = false;
        }
    }

    public void NewTurn(Component that)
    {
        isTurn = true;
    }

    public void ChangeBall(int i)
    {
        switch (i)
        {
            default: currentBall = ballPrefab; break;
            case 1: currentBall = otherBallPrefab; break;
            case 2: currentBall = cloneBallPrefab; break;
            case 3: currentBall = bombBallPrefab; break;
        }
    }
}
