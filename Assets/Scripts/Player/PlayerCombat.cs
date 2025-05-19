using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public float LightAttackCost_ = 5f;
    public float HeavyAttackCost_ = 10f;

    private Animator Animator_;
    private PlayerStamina Stamina_;
    private bool IsAttacking_;

    private void Awake()
    {
        Animator_ = GetComponentInChildren<Animator>();
        Stamina_ = GetComponent<PlayerStamina>();
    }

    public void OnLightAttack_(InputAction.CallbackContext context)
    {
        if (context.started)
            Debug.Log("Input recibido: Ataque débil");

        if (context.started && !IsAttacking_ && Stamina_.UseStamina_(LightAttackCost_))
        {
            Animator_.SetTrigger("Attack_Light");
            StartAttack_();
        }
    }


    public void OnHeavyAttack_(InputAction.CallbackContext context)
    {
        if (context.started && !IsAttacking_ && Stamina_.UseStamina_(HeavyAttackCost_))
        {
            Animator_.SetTrigger("Attack_Heavy");
            StartAttack_();
        }
    }

    private void StartAttack_()
    {
        IsAttacking_ = true;
        Invoke(nameof(EndAttack_), 0.75f); // duración aproximada, se ajusta según la animación
    }

    private void EndAttack_()
    {
        IsAttacking_ = false;
    }
}
// © 2025 KOIYOT. All rights reserved.