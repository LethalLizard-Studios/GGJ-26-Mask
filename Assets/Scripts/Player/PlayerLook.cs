using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public bool canLook = true;

    public Transform cameraPivot;

    public float mouseSensitivity = 100f;
    public float clampAngle = 85f;

    float xRotation = 0f;

    [SerializeField] private InputSystem_Actions m_InputActions;

    private void Awake()
    {
        m_InputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        m_InputActions.Player.Enable();
    }

    private void OnDisable()
    {
        m_InputActions.Player.Disable();
    }

    private void Update()
    {
        if (!canLook) return;

        Vector2 look = m_InputActions.Player.Look.ReadValue<Vector2>();

        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        // Rotate player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
