using EchoesOfAether.Input;
using UnityEngine;

namespace EchoesOfAether.Player
{
    /// <summary>
    /// Third-person movement built on Unity's CharacterController (CharacterController.Move).
    /// Handles camera-relative movement, walk/sprint, rotation toward the movement
    /// direction, jumping (with coyote time and jump buffering), gravity and grounding.
    /// No Rigidbody is used.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader input;
        [Tooltip("Camera used for camera-relative movement. If empty, Camera.main is used.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Speed")]
        [SerializeField, Min(0f)] private float walkSpeed = 3.5f;
        [SerializeField, Min(0f)] private float sprintSpeed = 6.5f;
        [Tooltip("How fast horizontal velocity reaches the target speed on the ground (m/s²).")]
        [SerializeField, Min(0f)] private float groundAcceleration = 20f;
        [Tooltip("Horizontal acceleration while airborne (m/s²). Lower = less air control.")]
        [SerializeField, Min(0f)] private float airAcceleration = 6f;
        [Tooltip("Time to smooth the character's yaw toward the movement direction.")]
        [SerializeField, Min(0f)] private float rotationSmoothTime = 0.1f;

        [Header("Jump & Gravity")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [Tooltip("Gravity acceleration (negative = down).")]
        [SerializeField] private float gravity = -20f;
        [Tooltip("Small downward velocity kept while grounded so isGrounded stays reliable.")]
        [SerializeField] private float groundedStickVelocity = -2f;
        [SerializeField] private float maxFallSpeed = -50f;
        [Tooltip("Grace period after leaving the ground during which a jump is still allowed.")]
        [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
        [Tooltip("How long a jump press is remembered before landing.")]
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.15f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float yawVelocity;
        private float lastGroundedTime = float.NegativeInfinity;
        private bool missingCameraWarned;

        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public float HorizontalSpeed => horizontalVelocity.magnitude;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (input == null) input = GetComponent<PlayerInputReader>();
            if (input == null)
            {
                Debug.LogError($"{nameof(PlayerMovement)} on '{name}' needs a {nameof(PlayerInputReader)} reference. Disabling.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            UpdateGrounding();
            HandleJump();
            ApplyGravity(dt);

            Vector3 moveDirection = GetCameraRelativeDirection(input.Move);
            UpdateHorizontalVelocity(moveDirection, input.Move.magnitude, dt);
            RotateTowards(moveDirection);

            Vector3 motion = (horizontalVelocity + Vector3.up * verticalVelocity) * dt;
            CollisionFlags flags = controller.Move(motion);

            // Stop upward velocity when hitting a ceiling.
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;
        }

        private void UpdateGrounding()
        {
            IsGrounded = controller.isGrounded;
            if (!IsGrounded) return;

            lastGroundedTime = Time.time;
            if (verticalVelocity < 0f) verticalVelocity = groundedStickVelocity;
        }

        private void HandleJump()
        {
            bool canJump = Time.time - lastGroundedTime <= coyoteTime && verticalVelocity <= 0f;
            if (!canJump) return;
            if (!input.TryConsumeJump(jumpBufferTime)) return;

            // v = sqrt(2 * h * |g|)
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
            lastGroundedTime = float.NegativeInfinity; // consume coyote time
            IsGrounded = false;
        }

        private void ApplyGravity(float dt)
        {
            if (IsGrounded && verticalVelocity <= 0f) return;
            verticalVelocity = Mathf.Max(verticalVelocity + gravity * dt, maxFallSpeed);
        }

        private Vector3 GetCameraRelativeDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude < 0.0001f) return Vector3.zero;

            Transform cam = ResolveCamera();
            Vector3 forward = cam != null ? cam.forward : transform.forward;
            Vector3 right = cam != null ? cam.right : transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * moveInput.y + right * moveInput.x).normalized;
        }

        private void UpdateHorizontalVelocity(Vector3 direction, float inputMagnitude, float dt)
        {
            IsSprinting = input.SprintHeld && inputMagnitude > 0.1f;
            float targetSpeed = (IsSprinting ? sprintSpeed : walkSpeed) * Mathf.Clamp01(inputMagnitude);
            Vector3 targetVelocity = direction * targetSpeed;

            float acceleration = IsGrounded ? groundAcceleration : airAcceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * dt);
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;

            float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref yawVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private Transform ResolveCamera()
        {
            if (cameraTransform != null) return cameraTransform;

            Camera main = Camera.main;
            if (main != null)
            {
                cameraTransform = main.transform;
                return cameraTransform;
            }

            if (!missingCameraWarned)
            {
                Debug.LogWarning($"{nameof(PlayerMovement)}: no camera assigned and no camera tagged MainCamera. Movement falls back to the player's own forward.", this);
                missingCameraWarned = true;
            }
            return null;
        }
    }
}
