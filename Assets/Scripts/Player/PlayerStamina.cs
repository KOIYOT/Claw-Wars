using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public float MaxStamina_ = 100f;
    public float CurrentStamina_ = 100f;
    public float RegenRate_ = 10f;

    public bool CanUseStamina_ => CurrentStamina_ > 0f;

    private void Start()
    {
        int slot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        if (slot >= 0 && SaveSystem.HasSaveData(slot))
        {
            SaveData data = SaveSystem.LoadFromSlot(slot);
            MaxStamina_ *= data.Stamina_;
        }

        CurrentStamina_ = MaxStamina_;
    }

    private void Update()
    {
        if (CurrentStamina_ < MaxStamina_)
        {
            CurrentStamina_ += RegenRate_ * Time.deltaTime;
            CurrentStamina_ = Mathf.Min(CurrentStamina_, MaxStamina_);
        }
    }

    public bool UseStamina_(float amount)
    {
        if (CurrentStamina_ >= amount)
        {
            CurrentStamina_ -= amount;
            Debug.Log("Stamina usada: " + amount + " | Actual: " + CurrentStamina_);
            return true;
        }

        Debug.Log("No hay suficiente stamina.");
        return false;
    }
}