using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    [Header("Camera Settings")]
    public float distance = 6f;
    public float heightOffset = 1.5f;
    public float mouseSensitivity = 3f;

    private float yaw = 0f;
    private float pitch = 15f;

    void Start()
    {
        // Locks the mouse cursor to the center of the game window and hides it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. Read the mouse inputs
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            // 2. Clamp the up/down rotation so the camera doesn't flip upside down
            pitch = Mathf.Clamp(pitch, -15f, 80f);

            // 3. Calculate the new rotation and position around the player
            Vector3 targetPosition = target.position + Vector3.up * heightOffset;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 position = targetPosition - (rotation * Vector3.forward * distance);

            // 4. Apply to the camera
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}