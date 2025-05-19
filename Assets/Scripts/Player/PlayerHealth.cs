using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float MaxHealth_ = 100f;
    public float CurrentHealth_ = 100f;

    private Animator Animator_;

    private void Awake()
    {
        CurrentHealth_ = MaxHealth_;
        Animator_ = GetComponentInChildren<Animator>();
    }

    public void TakeDamage_(float amount)
    {
        CurrentHealth_ -= amount;
        Debug.Log("Daño recibido: " + amount + " | Vida actual: " + CurrentHealth_);

        if (CurrentHealth_ <= 0f)
        {
            CurrentHealth_ = 0f;
            Die_();
        }
    }

    private void Die_()
    {
        Debug.Log("El jugador ha muerto.");
        Animator_.SetTrigger("Die");
    }
}

// © 2025 KOIYOT. All rights reserved.