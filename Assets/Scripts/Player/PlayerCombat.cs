using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public float LightAttackCost_ = 5f;
    public float HeavyAttackCost_ = 10f;
    public GameObject HitboxObject_;

    private Animator Animator_;
    private PlayerStamina Stamina_;
    private bool IsAttacking_;
    private float Strength_ = 1f;

    private void Awake()
    {
        Animator_ = GetComponentInChildren<Animator>();
        Stamina_ = GetComponent<PlayerStamina>();

        int slot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        if (slot >= 0 && SaveSystem.HasSaveData(slot))
        {
            SaveData data = SaveSystem.LoadFromSlot(slot);
            Strength_ = data.Strength_;
        }
    }

    public void OnLightAttack_(InputAction.CallbackContext context)
    {
        PlayerBlock block = GetComponent<PlayerBlock>();
        if (block != null && block.IsBlocking_) return;

        if (context.started && !IsAttacking_ && Stamina_.UseStamina_(LightAttackCost_))
        {
            Animator_.SetTrigger("Attack_Light");
            StartAttack_();
        }
    }

    public void OnHeavyAttack_(InputAction.CallbackContext context)
    {
        PlayerBlock block = GetComponent<PlayerBlock>();
        if (block != null && block.IsBlocking_) return;

        if (context.started && !IsAttacking_ && Stamina_.UseStamina_(HeavyAttackCost_))
        {
            Animator_.SetTrigger("Attack_Heavy");
            StartAttack_();
        }
    }

    private void StartAttack_()
    {
        IsAttacking_ = true;

        if (HitboxObject_ != null)
        {
            AttackHitbox hitbox = HitboxObject_.GetComponent<AttackHitbox>();
            if (hitbox != null)
            {
                hitbox.SetDamageMultiplier(Strength_);
                hitbox.ActivateHitbox_();
            }
        }

        Invoke(nameof(EndAttack_), 0.75f);
    }

    private void EndAttack_()
    {
        if (HitboxObject_ != null)
        {
            AttackHitbox hitbox = HitboxObject_.GetComponent<AttackHitbox>();
            if (hitbox != null)
                hitbox.DeactivateHitbox_();
        }

        IsAttacking_ = false;
    }
}
