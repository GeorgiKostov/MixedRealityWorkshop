using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gallery helper: toggle each UI-pattern station on/off with the number keys (1, 2, 3, ...).
/// Lets students isolate one pattern at a time instead of having all six competing for attention.
///
/// Each <see cref="ToggleSlot"/> maps one number key to one or more GameObjects (e.g. a panel plus
/// the object it labels). Stations start hidden by default (<see cref="startActive"/> = false).
/// </summary>
[DisallowMultipleComponent]
public class UIPatternToggleManager : MonoBehaviour
{
    [Serializable]
    public class ToggleSlot
    {
        [Tooltip("Label for readability in the Inspector only.")]
        public string name;
        [Tooltip("GameObjects toggled together by this slot's number key.")]
        public GameObject[] targets;
    }

    [Tooltip("Slot index 0 = key '1', slot 1 = key '2', and so on (up to '9').")]
    [SerializeField] private ToggleSlot[] slots;

    [Tooltip("Whether the stations are visible at scene start. Off by default.")]
    [SerializeField] private bool startActive = false;

    private void Start()
    {
        // Force every slot into the default state so the scene is deterministic on load.
        foreach (var slot in slots)
        {
            SetSlot(slot, startActive);
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || slots == null)
        {
            return;
        }

        var count = Mathf.Min(slots.Length, 9);
        for (var i = 0; i < count; i++)
        {
            // Key.Digit1 .. Key.Digit9 are sequential, so offset by the slot index.
            if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
            {
                ToggleSlotAt(i);
            }
        }
    }

    private void ToggleSlotAt(int index)
    {
        var slot = slots[index];
        if (slot == null || slot.targets == null || slot.targets.Length == 0)
        {
            return;
        }

        // Base the new state on the first target so the whole slot stays in sync.
        var first = slot.targets[0];
        var newState = first == null || !first.activeSelf;
        SetSlot(slot, newState);
        Debug.Log($"[UIPatternToggleManager] '{slot.name}' (key {index + 1}) -> {(newState ? "ON" : "OFF")}");
    }

    private static void SetSlot(ToggleSlot slot, bool active)
    {
        if (slot == null || slot.targets == null)
        {
            return;
        }

        foreach (var go in slot.targets)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }
}
