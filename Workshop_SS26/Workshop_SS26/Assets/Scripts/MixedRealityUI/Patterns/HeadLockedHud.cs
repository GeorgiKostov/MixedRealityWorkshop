using UnityEngine;

/// <summary>
/// Pattern: HEAD-LOCKED HUD - shown here deliberately as an ANTI-PATTERN to contrast with the
/// body-locked / FOV-locked followers.
///
/// The panel is rigidly pinned to the head: it never moves relative to your view, like a sticker on
/// a visor. It feels stable for a split second and then becomes tiring and slightly nauseating,
/// because real-world objects never track your head perfectly and your vestibular system notices.
///
/// Field guidance (why this is usually wrong, and the few times it's right):
///  - Avoid head-locking rich, dwell-on content. Body-lock or world-lock it instead.
///  - If you must head-lock (e.g. a reticle, a brief toast, a fade-to-black), keep it tiny, in the
///    periphery, and short-lived.
///  - Never head-lock text the user has to read for more than a moment.
/// </summary>
[DisallowMultipleComponent]
public class HeadLockedHud : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform viewer;

    [Tooltip("Offset from the head, in head-local space. Z is forward distance.")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.2f, 1.5f);

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

        // Rigidly copy the head pose - this is exactly what makes it uncomfortable.
        transform.position = viewer.TransformPoint(localOffset);
        transform.rotation = viewer.rotation;
    }
}
