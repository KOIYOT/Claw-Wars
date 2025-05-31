using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform Player_;
    private NavMeshAgent Agent_;
    private Animator Animator_;

    [Header("Rangos")]
    public float DetectionRange_ = 7f;
    public float AttackRange_ = 2f;
    public float LostSightRange_ = 10f;
    public float WanderRadius_ = 5f;
    public float WanderDelay_ = 4f;

    [Header("Velocidades")]
    public float WalkSpeed_ = 2f;
    public float RunSpeed_ = 5f;
    public float RunThreshold_ = 5f; // Si el jugador está más lejos que esto, corre

    private float WanderTimer_;
    private bool IsChasing_;

    void Awake()
    {
        Agent_ = GetComponent<NavMeshAgent>();
        Animator_ = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Player_ == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                Player_ = playerObj.transform;
            else
                return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player_.position);

        // Detectado y persiguiendo
        if (distanceToPlayer <= DetectionRange_)
        {
            Agent_.SetDestination(Player_.position);
            IsChasing_ = true;

            if (distanceToPlayer > RunThreshold_)
            {
                Agent_.speed = RunSpeed_;
                Animator_?.SetBool("IsRunning", true);
            }
            else
            {
                Agent_.speed = WalkSpeed_;
                Animator_?.SetBool("IsRunning", false);
            }
        }
        // Si estaba persiguiendo pero te perdió
        else if (IsChasing_ && distanceToPlayer > LostSightRange_)
        {
            IsChasing_ = false;
            WanderTimer_ = WanderDelay_;
            Agent_.speed = WalkSpeed_;
            Animator_?.SetBool("IsRunning", false);
        }
        // Patrullaje
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
            Animator_?.SetBool("IsRunning", false);
        }

        Animator_?.SetFloat("Speed", Agent_.velocity.magnitude);
    }

    Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            return hit.position;
        return transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, DetectionRange_);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange_);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, WanderRadius_);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, LostSightRange_);
    }
}
