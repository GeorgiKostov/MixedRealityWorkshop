using UnityEngine;

/// <summary>
/// Pattern: PROXIMITY REVEAL / FADE.
/// The panel is hidden at a distance and fades (and gently scales) in as the user approaches.
///
/// Field best practice baked in here:
///  - Use a <see cref="CanvasGroup"/> for the fade (one alpha for the whole panel) and turn off
///    <c>interactable</c>/<c>blocksRaycasts</c> while faded so you can't click an invisible panel.
///  - Two distances (visible/hidden) instead of one give a hysteresis band, so a user hovering near
///    the threshold doesn't get a flickering panel.
///  - Fade over time (MoveTowards) rather than snapping - sudden appearance reads as a glitch in XR.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class ProximityRevealCanvas : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform viewer;

    [Header("Reveal band (m)")]
    [Tooltip("At or below this distance the panel is fully visible.")]
    [SerializeField] private float fullyVisibleDistance = 2.5f;
    [Tooltip("At or beyond this distance the panel is fully hidden. The gap up to 'visible' is the hysteresis band.")]
    [SerializeField] private float fullyHiddenDistance = 5f;

    [Header("Motion")]
    [Tooltip("Scale while hidden (1 = no scale punch). A small value makes it 'grow' into place.")]
    [SerializeField] private float hiddenScale = 0.85f;
    [SerializeField] private float fadeSpeed = 6f;
    [SerializeField] private bool faceViewer = true;

    private CanvasGroup group;
    private Vector3 authoredScale;
    private float shown; // smoothed 0..1

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
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

        var distance = Vector3.Distance(viewer.position, transform.position);
        // 1 when at/closer than 'visible', 0 when at/further than 'hidden'.
        var target = 1f - Mathf.InverseLerp(fullyVisibleDistance, fullyHiddenDistance, distance);

        shown = Mathf.MoveTowards(shown, target, fadeSpeed * Time.deltaTime);
        group.alpha = shown;
        group.interactable = shown > 0.9f;
        group.blocksRaycasts = shown > 0.9f;
        transform.localScale = authoredScale * Mathf.Lerp(hiddenScale, 1f, shown);

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

    private void OnValidate()
    {
        fullyHiddenDistance = Mathf.Max(fullyVisibleDistance + 0.1f, fullyHiddenDistance);
    }
}
