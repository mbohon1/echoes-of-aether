using EchoesOfAether.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace EchoesOfAether.CameraControl
{
    /// <summary>
    /// Feeds look input into the camera target that a Cinemachine camera follows.
    ///
    /// Intended Cinemachine 3.x setup on the CinemachineCamera:
    ///   Tracking Target   = CameraTarget (child of the Player)
    ///   Position Control  = Third Person Follow
    ///   Rotation Control  = Rotate With Follow Target
    ///
    /// This script only rotates the CameraTarget (yaw/pitch). Cinemachine does all
    /// follow, orbit offset, damping and collision work — there is no custom
    /// camera follow system here.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerCameraInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader input;
        [Tooltip("Transform the Cinemachine camera follows. Usually an empty child of the Player at head height.")]
        [SerializeField] private Transform cameraTarget;
        [Tooltip("Optional. If assigned and its Tracking Target is empty, it is set to Camera Target on Start.")]
        [SerializeField] private CinemachineCamera cinemachineCamera;

        [Header("Sensitivity")]
        [Tooltip("Degrees of rotation per pixel of mouse movement.")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [Tooltip("Degrees per second at full stick deflection.")]
        [SerializeField, Min(0f)] private float stickSensitivity = 180f;
        [SerializeField] private bool invertY;

        [Header("Pitch Limits (degrees)")]
        [SerializeField, Range(-89f, 0f)] private float minPitch = -40f;
        [SerializeField, Range(0f, 89f)] private float maxPitch = 70f;

        private float yaw;
        private float pitch;

        private void Awake()
        {
            if (input == null) input = GetComponentInParent<PlayerInputReader>();

            if (input == null || cameraTarget == null)
            {
                Debug.LogError($"{nameof(PlayerCameraInput)} on '{name}' needs both a {nameof(PlayerInputReader)} and a Camera Target. Disabling.", this);
                enabled = false;
                return;
            }

            Vector3 euler = cameraTarget.eulerAngles;
            yaw = euler.y;
            pitch = NormalizeAngle(euler.x);
        }

        private void Start()
        {
            if (cinemachineCamera != null && cinemachineCamera.Follow == null)
                cinemachineCamera.Follow = cameraTarget;
        }

        private void LateUpdate()
        {
            Vector2 look = input.LookPointerDelta * mouseSensitivity
                           + input.LookStick * (stickSensitivity * Time.deltaTime);

            yaw += look.x;
            pitch += invertY ? look.y : -look.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            yaw = Mathf.Repeat(yaw, 360f);

            // World-space rotation so the target does not inherit the player's turning.
            cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}
