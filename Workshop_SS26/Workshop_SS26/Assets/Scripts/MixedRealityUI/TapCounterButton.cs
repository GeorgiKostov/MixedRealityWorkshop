using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tiny demo behaviour so the world-space button does something visible when clicked:
/// it counts taps and writes the total into a label. Keeps the UI interactive while the
/// students focus on the scaling / follow behaviour around it.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class TapCounterButton : MonoBehaviour
{
    [Tooltip("Label updated with the tap count. If empty, only logs to the console.")]
    [SerializeField] private TMP_Text countLabel;
    [SerializeField] private string format = "Taps: {0}";

    private Button button;
    private int count;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
        Refresh();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        count++;
        Refresh();
        Debug.Log($"[TapCounterButton] clicked {count} time(s)");
    }

    private void Refresh()
    {
        if (countLabel != null)
        {
            countLabel.text = string.Format(format, count);
        }
    }
}
