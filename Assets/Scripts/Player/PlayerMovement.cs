using UnityEngine;
using DG.Tweening;

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
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0;

        if (wantsToSprint != isSprinting)
        {
            isSprinting = wantsToSprint;

            if (playerCamera != null)
            {
                float targetFOV = isSprinting ? sprintFOV : normalFOV;
                fovTween?.Kill();
                fovTween = playerCamera.DOFieldOfView(targetFOV, fovTweenTime)
                                       .SetEase(Ease.OutSine);
            }
        }
    }

    void HandleMovement()
    {
        if (!canMove)
        {
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move.Normalize();

        // Gravity
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }
}
