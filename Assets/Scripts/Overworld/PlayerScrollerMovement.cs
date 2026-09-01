using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScrollerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    private bool canMove = true;

    void Update()
    {
        if (!canMove) return;

        float horizontalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontalInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontalInput += 1f;
        }

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public void SetMovementState(bool state)
    {
        canMove = state;
    }
}