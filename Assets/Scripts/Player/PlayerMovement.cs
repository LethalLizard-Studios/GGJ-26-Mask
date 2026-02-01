using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("FOV Settings")]
    public float sprintFOV = 75f;
    public float normalFOV = 60f;
    public float fovTweenTime = 0.25f;

    private CharacterController controller;
    private float verticalVelocity;
    private bool isSprinting;
    private Tween fovTween;

    public bool canMove = true;

    [SerializeField] private InputSystem_Actions m_InputActions;

    private void Awake()
    {
        m_InputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        m_InputActions.Player.Enable();
    }

    private void OnDisable()
    {
        m_InputActions.Player.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
    }

    void HandleInput()
    {
        if (!canMove)
        {
            if (isSprinting)
            {
                isSprinting = false;
                UpdateFovForSprintState();
            }
            return;
        }

        bool sprintHeld = m_InputActions.Player.Sprint.IsPressed();
        Vector2 moveRaw = m_InputActions.Player.Move.ReadValue<Vector2>();
        bool movingForward = moveRaw.y > 0.01f;

        bool wantsToSprint = sprintHeld && movingForward;

        if (wantsToSprint != isSprinting)
        {
            isSprinting = wantsToSprint;
            UpdateFovForSprintState();
        }
    }

    void HandleMovement()
    {
        if (!canMove)
        {
            return;
        }

        Vector2 moveRaw = m_InputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * moveRaw.x + transform.forward * moveRaw.y;
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void UpdateFovForSprintState()
    {
        if (playerCamera == null) return;

        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        fovTween?.Kill();
        fovTween = playerCamera.DOFieldOfView(targetFOV, fovTweenTime)
                               .SetEase(Ease.OutSine);
    }
}