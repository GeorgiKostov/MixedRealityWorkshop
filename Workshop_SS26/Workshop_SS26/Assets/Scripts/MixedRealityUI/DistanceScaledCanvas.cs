using UnityEngine;

/// <summary>
/// Distance-dependent UI scaling for a world-space canvas.
///
/// Teaching goal: in Mixed Reality a panel that sits at a fixed world position appears
/// smaller the further the user walks away. To keep text and buttons legible we scale the
/// panel up with distance so it keeps (roughly) the same *apparent* size on screen - the
/// same angular size the user perceived at the reference distance.
///
/// Two thresholds keep this sane:
///  - below <see cref="minDistance"/> the panel stops shrinking (so it never gets tiny when
///    the user leans in),
///  - beyond <see cref="maxDistance"/> the panel stops growing (so a user 50 m away does not
///    see a building-sized billboard).
///
/// The panel keeps its authored world position; only its scale (and optional facing) change.
/// </summary>
[DisallowMultipleComponent]
public class DistanceScaledCanvas : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Who we measure distance to. Defaults to Camera.main if left empty.")]
    [SerializeField] private Transform viewer;

    [Header("Scaling")]
    [Tooltip("Distance (m) at which the panel keeps the scale you authored in the editor.")]
    [SerializeField] private float referenceDistance = 2f;

    [Header("Thresholds (m)")]
    [Tooltip("Closer than this and the panel stops shrinking - the lower size cap.")]
    [SerializeField] private float minDistance = 1f;
    [Tooltip("Further than this and the panel stops growing - the upper size cap.")]
    [SerializeField] private float maxDistance = 10f;

    [Header("Facing")]
    [Tooltip("Rotate the panel to face the viewer so it is always readable head-on.")]
    [SerializeField] private bool faceViewer = true;
    [Tooltip("Keep the panel vertical (ignore the viewer's height) - avoids tilting up/down.")]
    [SerializeField] private bool keepUpright = true;

    private Vector3 authoredScale;

    [Header("Debug (read-only)")]
    [SerializeField] private float currentDistance;
    [SerializeField] private float currentScaleFactor = 1f;

    private void Awake()
    {
        authoredScale = transform.localScale;
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

        currentDistance = Vector3.Distance(viewer.position, transform.position);

        // Clamp the distance into the legible band, then scale linearly with it. Scaling
        // proportional to distance keeps the panel's apparent (angular) size constant.
        var clamped = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        currentScaleFactor = clamped / Mathf.Max(0.0001f, referenceDistance);
        transform.localScale = authoredScale * currentScaleFactor;

        if (faceViewer)
        {
            var toViewer = transform.position - viewer.position;
            if (keepUpright)
            {
                toViewer.y = 0f;
            }

            if (toViewer.sqrMagnitude > 0.0001f)
            {
                // Canvas forward (+Z) is its back face, so look along the away-from-viewer
                // direction to present the front of the panel.
                transform.rotation = Quaternion.LookRotation(toViewer.normalized, Vector3.up);
            }
        }
    }

    private void OnValidate()
    {
        minDistance = Mathf.Max(0.01f, minDistance);
        maxDistance = Mathf.Max(minDistance, maxDistance);
        referenceDistance = Mathf.Clamp(referenceDistance, minDistance, maxDistance);
    }
}
