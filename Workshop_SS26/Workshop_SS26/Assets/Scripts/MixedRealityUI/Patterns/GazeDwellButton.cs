using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pattern: GAZE + DWELL.
/// Hands-free selection: look at the target and hold; a radial timer fills and fires on completion.
/// On a desktop the mouse-look direction is the "gaze", so this is fully testable without a headset.
///
/// Field best practice baked in here:
///  - Angle-based targeting (cone around the view forward) is more forgiving than a pixel-perfect ray
///    and matches how head-gaze cursors actually feel.
///  - Show continuous progress (the radial fill) so dwell never feels like a hidden wait.
///  - Drain progress faster than it fills when the user looks away, so a glance doesn't creep toward a
///    false activation.
///  - A short cooldown after firing prevents an unwanted repeat while the user is still looking.
/// </summary>
[DisallowMultipleComponent]
public class GazeDwellButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform viewer;
    [Tooltip("Radial-filled Image used as the dwell timer. Set up automatically if assigned.")]
    [SerializeField] private Image dwellFill;
    [SerializeField] private TMP_Text label;

    [Header("Gaze targeting")]
    [Tooltip("Half-angle (deg) of the gaze cone that counts as 'looking at me'.")]
    [SerializeField] private float maxGazeAngle = 7f;
    [SerializeField] private float maxDistance = 8f;

    [Header("Dwell")]
    [SerializeField] private float dwellDuration = 1.2f;
    [Tooltip("How much faster progress drains than it fills when not gazed at.")]
    [SerializeField] private float releaseMultiplier = 2f;
    [SerializeField] private float cooldown = 0.4f;

    [Header("Labels")]
    [SerializeField] private string idleText = "Look here";
    [SerializeField] private string activatedText = "Activated x{0}";

    private float progress;
    private float cooldownLeft;
    private int count;

    private void Awake()
    {
        if (viewer == null && Camera.main != null)
        {
            viewer = Camera.main.transform;
        }

        if (dwellFill != null)
        {
            dwellFill.type = Image.Type.Filled;
            dwellFill.fillMethod = Image.FillMethod.Radial360;
            dwellFill.fillAmount = 0f;
        }

        if (label != null)
        {
            label.text = idleText;
        }
    }

    private void Update()
    {
        if (viewer == null)
        {
            return;
        }

        if (cooldownLeft > 0f)
        {
            cooldownLeft -= Time.deltaTime;
        }

        var gazed = cooldownLeft <= 0f && IsGazedAt();
        var rate = (gazed ? 1f : -releaseMultiplier) / Mathf.Max(0.01f, dwellDuration);
        progress = Mathf.Clamp01(progress + rate * Time.deltaTime);

        if (dwellFill != null)
        {
            dwellFill.fillAmount = progress;
        }

        if (progress >= 1f)
        {
            Activate();
        }
    }

    private bool IsGazedAt()
    {
        var to = transform.position - viewer.position;
        if (to.magnitude > maxDistance)
        {
            return false;
        }

        return Vector3.Angle(viewer.forward, to) <= maxGazeAngle;
    }

    private void Activate()
    {
        count++;
        progress = 0f;
        cooldownLeft = cooldown;
        if (dwellFill != null)
        {
            dwellFill.fillAmount = 0f;
        }

        if (label != null)
        {
            label.text = string.Format(activatedText, count);
        }

        Debug.Log($"[GazeDwellButton] activated {count} time(s)");
    }
}
