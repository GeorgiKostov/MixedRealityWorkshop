using UnityEngine;

/// <summary>
/// Pattern: DISTANCE LOD / PROGRESSIVE DISCLOSURE.
/// Legibility is not only about size. Far away we show a stripped-back "glance" view
/// (a title or icon); up close we swap in the full-detail view (body text, buttons).
///
/// Field best practice baked in here:
///  - Swap *content*, not just scale - a wall of small text is unreadable at distance no matter
///    how big you make it.
///  - Use a hysteresis margin around the switch distance so the view doesn't flip back and forth
///    when the user stands right on the threshold.
/// </summary>
[DisallowMultipleComponent]
public class DistanceLODCanvas : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform viewer;
    [Tooltip("Shown when the user is close (full detail).")]
    [SerializeField] private GameObject nearView;
    [Tooltip("Shown when the user is far (glanceable summary).")]
    [SerializeField] private GameObject farView;

    [Header("Switch (m)")]
    [SerializeField] private float switchDistance = 4f;
    [Tooltip("Dead band around the switch distance to stop flip-flopping.")]
    [SerializeField] private float hysteresis = 0.6f;

    [SerializeField] private bool faceViewer = true;

    private bool showingNear = true;

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

        var distance = Vector3.Distance(viewer.position, transform.position);

        if (showingNear && distance > switchDistance + hysteresis)
        {
            showingNear = false;
        }
        else if (!showingNear && distance < switchDistance - hysteresis)
        {
            showingNear = true;
        }

        if (nearView != null)
        {
            nearView.SetActive(showingNear);
        }

        if (farView != null)
        {
            farView.SetActive(!showingNear);
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
