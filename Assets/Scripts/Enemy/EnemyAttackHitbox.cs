using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
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

        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage_(Damage_);
                Debug.Log("El jugador recibió daño del enemigo: " + Damage_);
            }
        }
    }

    public void ActivateHitbox_()
    {
        Debug.Log("Hitbox de enemigo ACTIVADA");
        IsActive_ = true;
        if (Collider_ != null) Collider_.enabled = true;
    }

    public void DeactivateHitbox_()
    {
        IsActive_ = false;
        if (Collider_ != null) Collider_.enabled = false;
    }
}