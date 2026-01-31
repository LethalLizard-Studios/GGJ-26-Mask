using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Transform cameraPivot;

    public float mouseSensitivity = 100f;
    public float clampAngle = 85f;

    float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
