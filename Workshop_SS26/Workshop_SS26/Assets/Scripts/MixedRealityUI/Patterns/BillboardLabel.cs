using UnityEngine;

/// <summary>
/// Pattern: BILLBOARD OBJECT LABEL (with leader line).
/// A floating label anchored to a world object that always turns to face the user and stays a
/// constant apparent size, with a thin line connecting it to the thing it describes. This is the
/// standard way to annotate machinery, POIs, or 3D data in MR.
///
/// Field best practice baked in here:
///  - Billboard toward the viewer so text is never read edge-on, but keep it upright (no roll/pitch)
///    so it doesn't tumble.
///  - Hold a constant apparent size between sensible min/max distances so the label stays legible
///    near and far (same idea as DistanceScaledCanvas, reused here).
///  - A leader line ties the label to its anchor so the association is unambiguous when several
///    labels are on screen.
/// </summary>
[DisallowMultipleComponent]
public class BillboardLabel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform viewer;
    [Tooltip("The world object this label describes (leader line target).")]
    [SerializeField] private Transform anchor;
    [SerializeField] private LineRenderer leader;

    [Header("Facing")]
    [SerializeField] private bool keepUpright = true;

    [Header("Constant apparent size")]
    [SerializeField] private bool constantScreenSize = true;
    [SerializeField] private float referenceDistance = 3f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 12f;

    private Vector3 authoredScale;

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

        var to = transform.position - viewer.position;
        if (keepUpright)
        {
            to.y = 0f;
        }

        if (to.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
        }

        if (constantScreenSize)
        {
            var d = Mathf.Clamp(Vector3.Distance(viewer.position, transform.position), minDistance, maxDistance);
            transform.localScale = authoredScale * (d / Mathf.Max(0.0001f, referenceDistance));
        }

        if (leader != null && anchor != null)
        {
            leader.positionCount = 2;
            leader.SetPosition(0, anchor.position);
            leader.SetPosition(1, transform.position);
        }
    }
}
