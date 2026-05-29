using UnityEngine;

/// <summary>
/// "Tag-along" world-space UI - a panel that lazily follows the user around, the way the
/// HoloLens shell menu or a Magic Leap status panel does.
///
/// Teaching goal: body-locked UI should stay reachable without being glued rigidly to the
/// head (which feels nauseating). The trick is a target pose a fixed distance in front of the
/// viewer, plus *smoothing* so the panel drifts to catch up instead of snapping. Turn your
/// head and the panel swings around a beat later, then settles in front of you.
/// </summary>
[DisallowMultipleComponent]
public class TagAlongCanvas : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Who we follow. Defaults to Camera.main if left empty.")]
    [SerializeField] private Transform viewer;

    [Header("Placement")]
    [Tooltip("How far in front of the viewer the panel wants to sit (m).")]
    [SerializeField] private float followDistance = 2f;
    [Tooltip("Vertical offset from the viewer's eye line (m). Negative drops it below eye level.")]
    [SerializeField] private float verticalOffset = -0.2f;
    [Tooltip("Follow only the viewer's yaw, keeping the panel level when they look up/down.")]
    [SerializeField] private bool ignorePitch = true;

    [Header("Smoothing")]
    [Tooltip("Position lag. Higher = slower, lazier follow.")]
    [SerializeField] private float positionSmoothTime = 0.35f;
    [Tooltip("How quickly the panel rotates to face the viewer.")]
    [SerializeField] private float rotationSmoothSpeed = 8f;
    [Tooltip("Dead zone (m): the panel only chases once it drifts further than this from its target.")]
    [SerializeField] private float repositionThreshold = 0.15f;

    private Vector3 velocity;

    private void Awake()
    {
        if (viewer == null && Camera.main != null)
        {
            viewer = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (viewer == null)
        {
            return;
        }

        var targetPosition = ComputeTargetPosition();

        // Dead zone: ignore tiny target movements so the panel sits still until the user
        // actually moves/turns enough - this is what makes a tag-along feel calm.
        if (Vector3.Distance(transform.position, targetPosition) > repositionThreshold)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, targetPosition, ref velocity, positionSmoothTime);
        }

        var toViewer = transform.position - viewer.position;
        toViewer.y = 0f;
        if (toViewer.sqrMagnitude > 0.0001f)
        {
            var targetRotation = Quaternion.LookRotation(toViewer.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }

    private Vector3 ComputeTargetPosition()
    {
        var forward = viewer.forward;
        if (ignorePitch)
        {
            forward.y = 0f;
            forward.Normalize();
        }

        return viewer.position + forward * followDistance + Vector3.up * verticalOffset;
    }
}
