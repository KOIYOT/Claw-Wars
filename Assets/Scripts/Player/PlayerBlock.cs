using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerBlock : MonoBehaviour
{
    public bool IsBlocking_ { get; private set; } = false;
    public Animator Animator_;
    private PlayerResistance Resistance_;

    private void Awake()
    {
        Resistance_ = GetComponent<PlayerResistance>();
    }

    public void OnBlock_(InputAction.CallbackContext context)
    {
        if (Resistance_ != null && Resistance_.CanBlock_)
        {
            IsBlocking_ = context.ReadValueAsButton();

            if (Animator_ != null)
            {
                Animator_.SetBool("IsBlocking", IsBlocking_);
            }
        }
        else
        {
            IsBlocking_ = false;

            if (Animator_ != null)
            {
                Animator_.SetBool("IsBlocking", false);
            }
        }
    }
}

// © 2025 KOIYOT. All rights reserved.