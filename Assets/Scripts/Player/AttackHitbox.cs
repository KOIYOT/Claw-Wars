using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float Damage_ = 10f;
    public bool IsActive_ = false;

    private Collider Collider_;

    private void Awake()
    {
        Collider_ = GetComponent<Collider>();
        Collider_.enabled = false;
        IsActive_ = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive_) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage_(Damage_);
            Debug.Log("Golpeó a enemigo: " + other.name);
        }
    }

    public void ActivateHitbox_()
    {
        Debug.Log("Hitbox ACTIVADA");
        IsActive_ = true;
        Collider_.enabled = true; // 👈 Ahora sí se activa físicamente
    }

    public void DeactivateHitbox_()
    {
        IsActive_ = false;
        Collider_.enabled = false; // 👈 Se apaga físicamente
    }
}
// © 2025 KOIYOT. All rights reserved.