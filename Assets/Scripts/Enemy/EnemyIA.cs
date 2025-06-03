using UnityEngine;
using UnityEngine.AI;

public class EnemyIA : MonoBehaviour
{
    public Transform Player_;
    private NavMeshAgent Agent_;
    private Animator Animator_;
    private EnemyHealth Health_;

    public float DetectionRange_ = 7f;
    public float AttackRange_ = 2f;
    public float LostSightRange_ = 10f;
    public float WanderRadius_ = 5f;
    public float WanderDelay_ = 4f;

    public float WalkSpeed_ = 2f;
    public float RunSpeed_ = 5f;
    public float RunThreshold_ = 5f;

    public float AttackCooldown_ = 2f;
    public float AttackDamage_ = 10f;
    private float AttackTimer_;
    private bool IsAttacking_;

    private float WanderTimer_;
    private bool IsChasing_;

    void Awake()
    {
        Agent_ = GetComponent<NavMeshAgent>();
        Animator_ = GetComponentInChildren<Animator>();
        Health_ = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (Health_ != null && Health_.IsDead) return;

        if (Player_ == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                Player_ = playerObj.transform;
            else
                return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player_.position);

        if (distanceToPlayer <= DetectionRange_)
        {
            IsChasing_ = true;

            if (distanceToPlayer <= AttackRange_)
            {
                Agent_.SetDestination(Player_.position); // Mantente cerca
                FaceTarget(Player_.position);

                if (!IsAttacking_ && AttackTimer_ <= 0f)
                    Attack_();
            }
            else
            {
                Agent_.SetDestination(Player_.position);
                Agent_.speed = (distanceToPlayer > RunThreshold_) ? RunSpeed_ : WalkSpeed_;
            }
        }
        else if (IsChasing_ && distanceToPlayer > LostSightRange_)
        {
            IsChasing_ = false;
            WanderTimer_ = WanderDelay_;
            Agent_.speed = WalkSpeed_;
        }
        else if (!IsChasing_)
        {
            WanderTimer_ -= Time.deltaTime;
            if (WanderTimer_ <= 0f)
            {
                Vector3 target = RandomNavmeshLocation(WanderRadius_);
                Agent_.SetDestination(target);
                WanderTimer_ = WanderDelay_;
            }

            Agent_.speed = WalkSpeed_;
        }

        AttackTimer_ -= Time.deltaTime;
        Animator_?.SetFloat("Speed", Agent_.velocity.magnitude);
    }

    void Attack_()
    {
        IsAttacking_ = true;
        AttackTimer_ = AttackCooldown_;

        string attackTrigger = (Random.value < 0.5f) ? "Attack_Light" : "Attack_Heavy";
        Animator_?.SetTrigger(attackTrigger);

        EnemyAttackHitbox hitbox = GetComponentInChildren<EnemyAttackHitbox>();
        if (hitbox != null)
        {
            Debug.Log("Hitbox de enemigo ACTIVADA");
            hitbox.ActivateHitbox_();
        }

        Invoke(nameof(ResetAttack_), 1f);
    }

    void ResetAttack_()
    {
        IsAttacking_ = false;

        EnemyAttackHitbox hitbox = GetComponentInChildren<EnemyAttackHitbox>();
        if (hitbox != null)
            hitbox.DeactivateHitbox_();
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            return hit.position;
        return transform.position;
    }
}
