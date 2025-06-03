using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttributeUpgrade : MonoBehaviour
{
    public TextMeshProUGUI UpgradePointsText_;
    public TextMeshProUGUI StrengthText_;
    public TextMeshProUGUI SpeedText_;
    public TextMeshProUGUI StaminaText_;

    public Button StrengthButton_;
    public Button SpeedButton_;
    public Button StaminaButton_;

    private SaveData CurrentData_;
    private int Slot_;

    public void Init(SaveData data)
    {
        CurrentData_ = data;
        Slot_ = PlayerPrefs.GetInt("LastUsedSlot", -1);
        UpdateUI_();

        StrengthButton_.onClick.RemoveAllListeners();
        SpeedButton_.onClick.RemoveAllListeners();
        StaminaButton_.onClick.RemoveAllListeners();

        StrengthButton_.onClick.AddListener(() => UpgradeAttribute_("Strength"));
        SpeedButton_.onClick.AddListener(() => UpgradeAttribute_("Speed"));
        StaminaButton_.onClick.AddListener(() => UpgradeAttribute_("Stamina"));

        // Pausar el juego y mostrar el mouse
        Time.timeScale = 0f;

        if (MouseLockCenter.Instance != null)
            MouseLockCenter.Instance.UnlockMouse();
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void UpgradeAttribute_(string attribute)
    {
        if (CurrentData_ == null || CurrentData_.UpgradePoints_ <= 0)
            return;

        switch (attribute)
        {
            case "Strength": CurrentData_.Strength_ += 1f; break;
            case "Speed": CurrentData_.Speed_ += 1f; break;
            case "Stamina": CurrentData_.Stamina_ += 1f; break;
        }

        CurrentData_.UpgradePoints_--;
        SaveSystem.SaveToSlot(Slot_, CurrentData_);

        UpdateUI_();

        // Cerrar panel automáticamente después de usar un punto
        gameObject.SetActive(false);
        Time.timeScale = 1f;

        if (MouseLockCenter.Instance != null)
            MouseLockCenter.Instance.LockMouse();
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void UpdateUI_()
    {
        if (CurrentData_ == null) return;

        UpgradePointsText_.text = $"Points: {CurrentData_.UpgradePoints_}";
        StrengthText_.text = $"Fuerza = {CurrentData_.Strength_}";
        SpeedText_.text = $"Velocidad = {CurrentData_.Speed_}";
        StaminaText_.text = $"Resistencia = {CurrentData_.Stamina_}";
    }
}
