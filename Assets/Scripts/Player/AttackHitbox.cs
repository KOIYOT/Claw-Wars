using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float Damage_ = 10f;
    public bool IsActive_ = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive_) return;
        Debug.Log("Hitbox activada: impacto con " + other.name);
    }

    public void ActivateHitbox_()
    {
        IsActive_ = true;
    }

    public void DeactivateHitbox_()
    {
        IsActive_ = false;
    }
}

// © 2025 KOIYOT. All rights reserved.