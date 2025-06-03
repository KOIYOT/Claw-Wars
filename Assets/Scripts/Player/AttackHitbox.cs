using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float Damage_ = 10f;
    public bool IsActive_ = false;

    private Collider Collider_;
    private float DamageMultiplier_ = 1f;

    private void Awake()
    {
        Collider_ = GetComponent<Collider>();
        Collider_.enabled = false;
        IsActive_ = false;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        DamageMultiplier_ = multiplier;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive_) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            float finalDamage = Damage_ * DamageMultiplier_;
            enemy.TakeDamage_(finalDamage);
            Debug.Log($"Golpeó a {other.name} por {finalDamage} de daño (mult: {DamageMultiplier_})");
        }
    }

    public void ActivateHitbox_()
    {
        Debug.Log("Hitbox ACTIVADA");
        IsActive_ = true;
        Collider_.enabled = true;
    }

    public void DeactivateHitbox_()
    {
        IsActive_ = false;
        Collider_.enabled = false;
    }
}