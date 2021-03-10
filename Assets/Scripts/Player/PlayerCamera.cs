using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public float xRotation = 0f;
    public PlayerMovementOneScript playerMovement;

    public void LateUpdate()
    {
        float mouse_X = Input.GetAxis("Mouse X");
        float mouse_Y = Input.GetAxis("Mouse Y");
        Vector2 mouseDirection = new Vector2(mouse_X, mouse_Y) * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseDirection.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseDirection.x); 
    }
}
