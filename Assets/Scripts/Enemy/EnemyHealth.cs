using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float MaxHealth_ = 50f;
    public float CurrentHealth_ = 50f;

    public Slider BarReal_;
    public Slider BarDelay_;
    public float BarLerpSpeed_ = 2f;

    private Animator Animator_;
    private bool IsDead_ = false;

    private void Awake()
    {
        CurrentHealth_ = MaxHealth_;
        Animator_ = GetComponentInChildren<Animator>();

        if (BarReal_ != null)
            BarReal_.value = 1f;

        if (BarDelay_ != null)
            BarDelay_.value = 1f;
    }

    private void Update()
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
    }

    public void TakeDamage_(float amount)
    {
        if (IsDead_) return;

        CurrentHealth_ -= amount;
        Debug.Log($"{gameObject.name} recibió {amount} de daño. Vida: {CurrentHealth_}");

        if (BarReal_ != null)
            BarReal_.value = CurrentHealth_ / MaxHealth_;

        if (CurrentHealth_ <= 0f)
        {
            Die_();
        }
    }

    private void Die_()
    {
        IsDead_ = true;
        Debug.Log($"{gameObject.name} ha muerto.");

        if (Animator_ != null)
        {
            Animator_.SetTrigger("Die");
            Destroy(transform.root.gameObject, 3.2f);

        }
    }
}
// © 2025 KOIYOT. All rights reserved.