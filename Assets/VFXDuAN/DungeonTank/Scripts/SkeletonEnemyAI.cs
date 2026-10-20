using System.Collections;
using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class SkeletonEnemyAI : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;

        [Header("Chase")]
        [Min(0.1f)] public float moveSpeed = 2.35f;
        [Min(0.1f)] public float detectionRange = 13f;
        [Min(0.1f)] public float attackRange = 1.75f;
        [Min(0.1f)] public float stopDistance = 1.55f;
        [Min(0.1f)] public float turnSpeed = 8f;

        [Header("Attack")]
        [Min(0.1f)] public float attackCooldown = 1.25f;
        [Min(1f)] public float attackDamage = 14f;
        [Range(0f, 1f)] public float damageDelay = 0.32f;
        public Animator animator;

        private float nextAttackTime;
        private SkeletonEnemyHealth health;
        private Coroutine attackRoutine;
        private bool attacking;

        private void Awake()
        {
            health = GetComponent<SkeletonEnemyHealth>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            // Enemy collision is trigger-only so skeletons can approach the player
            // without physically shoving the Rigidbody character around.
            Collider[] cols = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in cols)
            {
                if (col != null)
                    col.isTrigger = true;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
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

            FaceTarget(toTarget);

            if (attacking)
            {
                SetMoveAnimator(0f);
                return;
            }

            if (distance > stopDistance)
            {
                float step = moveSpeed * Time.deltaTime;
                float allowed = Mathf.Max(0f, distance - stopDistance);
                transform.position += transform.forward * Mathf.Min(step, allowed);
                SetMoveAnimator(1f);
            }
            else
            {
                SetMoveAnimator(0f);
            }

            if (distance <= attackRange && Time.time >= nextAttackTime)
                BeginAttack();
        }

        private void FaceTarget(Vector3 toTarget)
        {
            if (toTarget.sqrMagnitude <= 0.0025f) return;
            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, Time.deltaTime * turnSpeed);
        }

        private void BeginAttack()
        {
            nextAttackTime = Time.time + attackCooldown;
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            attacking = true;
            SetMoveAnimator(0f);
            TryTrigger("Attack");

            if (damageDelay > 0f)
                yield return new WaitForSeconds(damageDelay);

            DealDamageIfStillInRange();

            float remaining = Mathf.Max(0.05f, attackCooldown - damageDelay);
            yield return new WaitForSeconds(remaining * 0.55f);
            attacking = false;
            attackRoutine = null;
        }

        private void DealDamageIfStillInRange()
        {
            if (target == null) return;

            CrusaderTankHealth player = target.GetComponent<CrusaderTankHealth>();
            if (player == null) player = target.GetComponentInParent<CrusaderTankHealth>();
            if (player == null || player.IsDead) return;

            Vector3 a = transform.position;
            Vector3 b = target.position;
            a.y = b.y = 0f;
            if (Vector3.Distance(a, b) <= attackRange + 0.25f)
                player.TakeDamage(attackDamage);
        }

        private void SetMoveAnimator(float value)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == "Speed" && p.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat("Speed", value, 0.1f, Time.deltaTime);
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
