using UnityEngine;

/// <summary>
/// Pattern: FOV-LOCKED FOLLOWER (a.k.a. "lazy follow" with a view-cone dead zone).
/// Compare with <c>TagAlongCanvas</c>: that one is always gently chasing. This one stays perfectly
/// still in the world while it remains inside your field of view, and only re-centers once you turn
/// far enough that it would leave the screen. This is the calmer, less distracting follow style and
/// is what most modern MR shells use for menus.
///
/// Field best practice baked in here:
///  - Don't move body-locked UI while the user can already see it - motion in the periphery is what
///    causes discomfort. Movement should be the exception, triggered by leaving the FOV cone.
///  - When it does move, ease (SmoothDamp) it to the new spot rather than teleporting.
///  - Re-center to a generous inner angle so it settles comfortably in view, not right at the edge.
/// </summary>
[DisallowMultipleComponent]
public class FovLockedFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform viewer;

    [Header("Placement")]
    [SerializeField] private float followDistance = 2.5f;
    [SerializeField] private float verticalOffset = 0f;

    [Header("View cone")]
    [Tooltip("Once the panel sits more than this angle (deg) off-centre, it starts re-centring.")]
    [SerializeField] private float maxViewAngle = 22f;
    [Tooltip("How quickly it eases to the new position once triggered.")]
    [SerializeField] private float repositionSmoothTime = 0.25f;

    [SerializeField] private bool faceViewer = true;

    private Vector3 velocity;
    private bool repositioning;

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

        var toPanel = transform.position - viewer.position;
        var offAngle = Vector3.Angle(viewer.forward, toPanel);

        // Leaving the comfortable cone? Start chasing back to centre.
        if (offAngle > maxViewAngle)
        {
            repositioning = true;
        }

        if (repositioning)
        {
            var forward = viewer.forward;
            forward.y = 0f;
            forward.Normalize();
            var target = viewer.position + forward * followDistance + Vector3.up * verticalOffset;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, repositionSmoothTime);

            // Settle once it is comfortably back inside the cone.
            var recentred = Vector3.Angle(viewer.forward, transform.position - viewer.position);
            if (recentred < maxViewAngle * 0.3f)
            {
                repositioning = false;
            }
        }

        if (faceViewer)
        {
            var to = transform.position - viewer.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
            }
        }
    }
}
