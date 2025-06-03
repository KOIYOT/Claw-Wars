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

        StrengthButton_.onClick.AddListener(() => UpgradeAttribute_("Strength"));
        SpeedButton_.onClick.AddListener(() => UpgradeAttribute_("Speed"));
        StaminaButton_.onClick.AddListener(() => UpgradeAttribute_("Stamina"));
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

        if (CurrentData_.UpgradePoints_ <= 0)
        {
            StrengthButton_.interactable = false;
            SpeedButton_.interactable = false;
            StaminaButton_.interactable = false;
        }
    }

    void UpdateUI_()
    {
        UpgradePointsText_.text = $"You have {CurrentData_.UpgradePoints_} points to upgrade";
        StrengthText_.text = $"Strength = {CurrentData_.Strength_}";
        SpeedText_.text = $"Speed = {CurrentData_.Speed_}";
        StaminaText_.text = $"Stamina = {CurrentData_.Stamina_}";
    }
}