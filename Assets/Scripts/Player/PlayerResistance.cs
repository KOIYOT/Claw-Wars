using UnityEngine;
using UnityEngine.UI;

public class PlayerResistance : MonoBehaviour
{
    [Header("Valores de Resistencia")]
    public float MaxResistance_ = 100f;
    public float CurrentResistance_;
    public float RegenDelay_ = 2f;
    public float BrokenRegenRate_ = 20f;

    [Header("UI")]
    public Slider BarReal_;
    public Slider BarDelay_;
    public float BarLerpSpeed_ = 2f;

    [Header("Estado")]
    public bool IsBroken_ = false;
    public bool CanBlock_ { get; private set; } = true;

    private float LastHitTime_;
    private bool WaitingToRegen_ = false;

    void Start()
    {
        CurrentResistance_ = MaxResistance_;
        if (BarReal_ != null) BarReal_.value = 1f;
        if (BarDelay_ != null) BarDelay_.value = 1f;
    }

    void Update()
    {
        if (BarDelay_ != null && BarReal_ != null)
        {
            float realValue = BarReal_.value;
            float currentValue = BarDelay_.value;
            if (currentValue > realValue)
            {
                BarDelay_.value = Mathf.Lerp(currentValue, realValue, Time.deltaTime * BarLerpSpeed_);
            }
        }

        if (IsBroken_)
        {
            RegenerateBroken_();
        }
        else if (!WaitingToRegen_ && Time.time - LastHitTime_ >= RegenDelay_)
        {
            Regenerate_();
        }
    }

    public void AbsorbHit_(float amount)
    {
        if (IsBroken_) return;

        CurrentResistance_ -= amount;
        LastHitTime_ = Time.time;

        if (BarReal_ != null)
            BarReal_.value = CurrentResistance_ / MaxResistance_;

        if (CurrentResistance_ <= 0)
        {
            IsBroken_ = true;
            CanBlock_ = false;
            WaitingToRegen_ = true;
            Invoke(nameof(StartBrokenRegen_), RegenDelay_);
            Debug.Log("¡Resistencia rota!");
        }
    }

    void StartBrokenRegen_()
    {
        WaitingToRegen_ = false;
    }

    void Regenerate_()
    {
        if (CurrentResistance_ < MaxResistance_)
        {
            CurrentResistance_ += BrokenRegenRate_ * Time.deltaTime;
            CurrentResistance_ = Mathf.Min(CurrentResistance_, MaxResistance_);
            if (BarReal_ != null)
                BarReal_.value = CurrentResistance_ / MaxResistance_;
        }
    }

    void RegenerateBroken_()
    {
        Regenerate_();
        if (CurrentResistance_ >= MaxResistance_)
        {
            CurrentResistance_ = MaxResistance_;
            IsBroken_ = false;
            CanBlock_ = true;
            Debug.Log("Resistencia restaurada por completo.");
        }
    }
}