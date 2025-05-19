using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public float DashDistance_ = 6f;
    public float DashDuration_ = 0.2f;
    public float DashCooldown_ = 1f;
    public float StaminaCost_ = 20f;

    private CharacterController Controller_;
    private PlayerStamina Stamina_;
    private Vector3 DashDirection_;
    private bool IsDashing_;
    private float DashTimer_;
    private float CooldownTimer_;

    private void Awake()
    {
        Controller_ = GetComponent<CharacterController>();
        Stamina_ = GetComponent<PlayerStamina>();
    }

    public void OnDash_(InputAction.CallbackContext context)
    {
        if (context.started && !IsDashing_ && CooldownTimer_ <= 0f && Stamina_.UseStamina_(StaminaCost_))
        {
            DashDirection_ = transform.forward;
            IsDashing_ = true;
            DashTimer_ = DashDuration_;
            CooldownTimer_ = DashCooldown_;
        }
    }

    private void Update()
    {
        if (CooldownTimer_ > 0f)
            CooldownTimer_ -= Time.deltaTime;

        if (IsDashing_)
        {
            Controller_.Move(DashDirection_ * (DashDistance_ / DashDuration_) * Time.deltaTime);
            DashTimer_ -= Time.deltaTime;

            if (DashTimer_ <= 0f)
                IsDashing_ = false;
        }
    }
}

// © 2025 KOIYOT. All rights reserved.