using UnityEngine;

public class SaveSlotGroupController : MonoBehaviour
{
    private SaveSlotButton[] slots;

    private void OnEnable()
    {
        RefreshAll();
    }

    private void Start()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        slots = GetComponentsInChildren<SaveSlotButton>();
        foreach (var slot in slots)
        {
            slot.RefreshDisplay();
        }
    }
}