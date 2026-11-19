using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfAether.Input
{
    /// <summary>
    /// Reads player input (move, look, jump, sprint) via the Unity Input System
    /// and exposes it as plain values. It never moves the player or the camera.
    ///
    /// Actions are built in code so the project does not depend on an
    /// .inputactions asset. Bindings: WASD/arrows + left stick (move),
    /// mouse delta + right stick (look), Space + gamepad South (jump),
    /// Left Shift + left stick press (sprint).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Cursor")]
        [Tooltip("Lock and hide the cursor while this component is enabled. Press Escape to release it.")]
        [SerializeField] private bool lockCursor = true;

        private InputAction moveAction;
        private InputAction lookPointerAction;
        private InputAction lookStickAction;
        private InputAction jumpAction;
        private InputAction sprintAction;

        private float lastJumpPressedTime = float.NegativeInfinity;

        /// <summary>Movement input, each axis in [-1, 1], magnitude clamped to 1.</summary>
        public Vector2 Move { get; private set; }

        /// <summary>Pointer (mouse) look delta in pixels for the current frame. Do NOT multiply by deltaTime.</summary>
        public Vector2 LookPointerDelta { get; private set; }

        /// <summary>Stick look input in [-1, 1]. This is a rate: multiply by deltaTime.</summary>
        public Vector2 LookStick { get; private set; }

        /// <summary>True while the sprint control is held.</summary>
        public bool SprintHeld { get; private set; }

        private void Awake()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");

            lookPointerAction = new InputAction("LookPointer", InputActionType.Value, "<Mouse>/delta");
            lookStickAction = new InputAction("LookStick", InputActionType.Value, "<Gamepad>/rightStick");

            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            sprintAction = new InputAction("Sprint", InputActionType.Button);
            sprintAction.AddBinding("<Keyboard>/leftShift");
            sprintAction.AddBinding("<Gamepad>/leftStickPress");
        }

        private void OnEnable()
        {
            jumpAction.performed += OnJumpPerformed;

            moveAction.Enable();
            lookPointerAction.Enable();
            lookStickAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();

            if (lockCursor) SetCursorLocked(true);
        }

        private void OnDisable()
        {
            jumpAction.performed -= OnJumpPerformed;

            moveAction.Disable();
            lookPointerAction.Disable();
            lookStickAction.Disable();
            jumpAction.Disable();
            sprintAction.Disable();

            Move = Vector2.zero;
            LookPointerDelta = Vector2.zero;
            LookStick = Vector2.zero;
            SprintHeld = false;

            if (lockCursor) SetCursorLocked(false);
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            lookPointerAction?.Dispose();
            lookStickAction?.Dispose();
            jumpAction?.Dispose();
            sprintAction?.Dispose();
        }

        private void Update()
        {
            Move = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
            SprintHeld = sprintAction.IsPressed();

            bool cursorFree = lockCursor && Cursor.lockState != CursorLockMode.Locked;
            LookPointerDelta = cursorFree ? Vector2.zero : lookPointerAction.ReadValue<Vector2>();
            LookStick = lookStickAction.ReadValue<Vector2>();

            if (lockCursor)
            {
                Keyboard keyboard = Keyboard.current;
                Mouse mouse = Mouse.current;
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                    SetCursorLocked(false);
                else if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                    SetCursorLocked(true);
            }
        }

        /// <summary>
        /// Returns true (and clears the request) if jump was pressed within the last
        /// <paramref name="bufferTime"/> seconds. Used for jump buffering.
        /// </summary>
        public bool TryConsumeJump(float bufferTime)
        {
            if (Time.time - lastJumpPressedTime > bufferTime) return false;
            lastJumpPressedTime = float.NegativeInfinity;
            return true;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            lastJumpPressedTime = Time.time;
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
