using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedVelocity = -2f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private Joystick joystick;

    private CharacterController controller;

    private Vector2 moveInput;
    private Vector3 verticalVelocity;

    private bool isGrounded;

    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        jumpAction?.action.Enable();

        if (jumpAction != null)
            jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        if (jumpAction != null)
            jumpAction.action.performed -= OnJump;

        moveAction?.action.Disable();
        jumpAction?.action.Disable();
    }

    private void Update()
    {
        ReadInput();
        CheckGrounded();
        Move();
        ApplyGravity();
        UpdateAnimation();
    }

    private void ReadInput()
    {
        Vector2 actionInput = moveAction != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        Vector2 joystickInput = joystick != null
            ? joystick.Direction
            : Vector2.zero;

        moveInput = joystickInput.sqrMagnitude > 0.01f
            ? joystickInput
            : actionInput;
    }

    private void CheckGrounded()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = groundedVelocity;
    }

    private void Move()
    {
        Vector3 moveDirection = GetCameraRelativeMoveDirection();

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private Vector3 GetCameraRelativeMoveDirection()
    {
        if (cameraTransform == null)
            return new Vector3(moveInput.x, 0f, moveInput.y);

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        return Vector3.ClampMagnitude(moveDirection, 1f);
    }

    private void ApplyGravity()
    {
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        TryJump();
    }

    public void OnBtnJumpPressed()
    {
        TryJump();
    }

    private void TryJump()
    {
        if (!isGrounded)
            return;

        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (animator != null)
        {
            animator.ResetTrigger(JumpHash);
            animator.SetTrigger(JumpHash);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        animator.SetFloat(MoveSpeedHash, moveInput.magnitude, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, isGrounded);
    }
}