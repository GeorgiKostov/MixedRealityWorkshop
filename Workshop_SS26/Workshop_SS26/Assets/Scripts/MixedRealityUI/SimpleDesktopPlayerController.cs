using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Minimal desktop test rig for trying out Mixed Reality UI on a flat keyboard/mouse setup:
/// WASD to move, mouse to look around. This stands in for an XR headset so students can
/// iterate on UI behaviour without putting a device on.
///
/// Put this on the Player root. The camera should be a child positioned at eye height.
/// Yaw (left/right) rotates the player body; pitch (up/down) rotates only the camera.
/// </summary>
[DisallowMultipleComponent]
public class SimpleDesktopPlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera child used for pitch (look up/down). Falls back to Camera.main child if left empty.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintMultiplier = 2f;
    [Tooltip("Keeps the player at a constant height - no gravity, ideal for a flat demo floor.")]
    [SerializeField] private bool lockHeight = true;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [Tooltip("Lock and hide the cursor on start. Press Escape to release it.")]
    [SerializeField] private bool lockCursorOnStart = true;

    private float pitch;
    private float fixedHeight;

    private void Awake()
    {
        if (cameraTransform == null)
        {
            var childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                cameraTransform = childCamera.transform;
            }
        }

        fixedHeight = transform.position.y;
        pitch = cameraTransform != null ? cameraTransform.localEulerAngles.x : 0f;
    }

    private void OnEnable()
    {
        if (lockCursorOnStart)
        {
            SetCursorLocked(true);
        }
    }

    private void Update()
    {
        HandleCursorToggle();
        HandleLook();
        HandleMovement();
    }

    private void HandleCursorToggle()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            SetCursorLocked(false);
        }

        // Click back into the game window to recapture the mouse.
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            SetCursorLocked(true);
        }
    }

    private void HandleLook()
    {
        var mouse = Mouse.current;
        if (mouse == null || Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        var delta = mouse.delta.ReadValue() * lookSensitivity;

        // Yaw turns the whole body so movement follows where we look.
        transform.Rotate(Vector3.up, delta.x, Space.Self);

        // Pitch only tilts the camera, clamped so we never flip over.
        if (cameraTransform != null)
        {
            pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        var input = new Vector2(
            (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
            (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));

        var speed = moveSpeed * (keyboard.leftShiftKey.isPressed ? sprintMultiplier : 1f);

        var move = (transform.right * input.x + transform.forward * input.y);
        if (lockHeight)
        {
            move.y = 0f;
        }

        transform.position += Vector3.ClampMagnitude(move, 1f) * (speed * Time.deltaTime);

        if (lockHeight)
        {
            var p = transform.position;
            p.y = fixedHeight;
            transform.position = p;
        }
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
