using UnityEngine;

public class CameraFollowHorizontal : MonoBehaviour
{
    public Transform target;       
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 10f, -3f); 

    private bool isFollowing = true;

    void LateUpdate()
    {
        if (!isFollowing || target == null) return;

        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, offset.y, offset.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }

    public void SetFollowing(bool state)
    {
        isFollowing = state;
    }
}