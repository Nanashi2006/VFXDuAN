using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class SkeletonEnemyAI : MonoBehaviour
    {
        public Transform target;
        [Min(0.1f)] public float moveSpeed = 2.4f;
        [Min(0.1f)] public float detectionRange = 14f;
        [Min(0.1f)] public float attackRange = 2.25f;
        [Min(0.1f)] public float attackCooldown = 1.2f;
        [Min(1f)] public float attackDamage = 16f;
        public Animator animator;

        private float nextAttackTime;
        private SkeletonEnemyHealth health;

        private void Awake()
        {
            health = GetComponent<SkeletonEnemyHealth>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (target == null)
            {
                CrusaderTankHealth player = FindAnyObjectByType<CrusaderTankHealth>();
                if (player != null) target = player.transform;
            }
        }

        private void Update()
        {
            if (target == null || (health != null && health.IsDead)) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > detectionRange)
            {
                SetMoveAnimator(0f);
                return;
            }

            if (distance > 0.05f)
            {
                Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desired, Time.deltaTime * 7f);
            }

            if (distance > attackRange)
            {
                transform.position += transform.forward * (moveSpeed * Time.deltaTime);
                SetMoveAnimator(1f);
            }
            else
            {
                SetMoveAnimator(0f);
                if (Time.time >= nextAttackTime)
                    Attack();
            }
        }

        private void Attack()
        {
            nextAttackTime = Time.time + attackCooldown;
            TryTrigger("Attack");

            CrusaderTankHealth player = target != null ? target.GetComponent<CrusaderTankHealth>() : null;
            if (player == null && target != null) player = target.GetComponentInParent<CrusaderTankHealth>();
            if (player != null) player.TakeDamage(attackDamage);
        }

        private void SetMoveAnimator(float value)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == "Speed" && p.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat("Speed", value);
                    return;
                }
                if (p.name == "Moving" && p.type == AnimatorControllerParameterType.Bool)
                    animator.SetBool("Moving", value > 0.1f);
            }
        }

        private void TryTrigger(string parameter)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == parameter && p.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(parameter);
                    return;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
