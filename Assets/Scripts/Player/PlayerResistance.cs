using UnityEngine;

public class PlayerResistance : MonoBehaviour
{
    public float MaxResistance_ = 50f;
    public float CurrentResistance_ = 50f;
    public float RegenRate_ = 10f;
    public float RegenDelay_ = 2f;

    private float RegenCooldown_;
    private bool IsBroken_;

    public bool CanBlock_ => !IsBroken_;

    private void Update()
    {
        if (!IsBroken_ && CurrentResistance_ < MaxResistance_)
        {
            if (RegenCooldown_ > 0f)
            {
                RegenCooldown_ -= Time.deltaTime;
            }
            else
            {
                CurrentResistance_ += RegenRate_ * Time.deltaTime;
                CurrentResistance_ = Mathf.Min(CurrentResistance_, MaxResistance_);
            }
        }

        if (IsBroken_ && CurrentResistance_ >= MaxResistance_)
        {
            IsBroken_ = false;
            Debug.Log("Resistencia recuperada.");
        }
    }

    public void AbsorbHit_(float amount)
    {
        if (IsBroken_) return;

        CurrentResistance_ -= amount;
        RegenCooldown_ = RegenDelay_;
        Debug.Log("Resistencia absorbió golpe. Restante: " + CurrentResistance_);

        if (CurrentResistance_ <= 0f)
        {
            IsBroken_ = true;
            CurrentResistance_ = 0f;
            Debug.Log("¡Resistencia rota!");
        }
    }
}

// © 2025 KOIYOT. All rights reserved.