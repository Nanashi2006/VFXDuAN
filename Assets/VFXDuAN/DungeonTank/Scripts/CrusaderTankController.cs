using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class CrusaderTankController : MonoBehaviour
    {
        [Header("Movement")]
        [Min(0.1f)] public float moveSpeed = 4.2f;
        [Min(1f)] public float turnSpeed = 125f;
        public Animator animator;

        [Header("Melee Combat")]
        public Transform attackPoint;
        [Min(0.1f)] public float attackRadius = 1.65f;
        [Min(1f)] public float attackDamage = 25f;
        [Min(0.1f)] public float attackCooldown = 0.72f;
        [Range(0f, 1f)] public float damageDelay = 0.22f;
        public LayerMask enemyLayers = ~0;
        public GameObject slashVFXPrefab;
        public GameObject hitVFXPrefab;

        // Kept only so older generated scenes keep loading without missing serialized fields.
        [HideInInspector] public Transform muzzle;
        [HideInInspector] public GameObject muzzleVFXPrefab;
        [HideInInspector] public GameObject impactVFXPrefab;

        private Rigidbody body;
        private float moveInput;
        private float turnInput;
        private float nextAttackTime;
        private bool gameplayEnabled = true;
        private bool attacking;
        private Coroutine attackRoutine;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (attackPoint == null)
                attackPoint = muzzle != null ? muzzle : CreateRuntimeAttackPoint();
        }

        private void Update()
        {
            ReadInput();
            UpdateAnimator();

            if (gameplayEnabled && AttackPressed() && Time.time >= nextAttackTime)
                BeginAttack();
        }

        private void FixedUpdate()
        {
            if (!gameplayEnabled || attacking) return;

            Vector3 delta = transform.forward * (moveInput * moveSpeed * Time.fixedDeltaTime);
            body.MovePosition(body.position + delta);

            Quaternion turn = Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);
            body.MoveRotation(body.rotation * turn);
        }

        public void SetGameplayEnabled(bool value)
        {
            gameplayEnabled = value;
            if (!value)
            {
                moveInput = 0f;
                turnInput = 0f;
                attacking = false;
                if (attackRoutine != null)
                {
                    StopCoroutine(attackRoutine);
                    attackRoutine = null;
                }
                SetSpeedParameter(0f);
            }
        }

        private void BeginAttack()
        {
            nextAttackTime = Time.time + attackCooldown;
            if (attackRoutine != null)
                StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            attacking = true;
            moveInput = 0f;
            SetSpeedParameter(0f);
            TryTrigger(AttackHash, "Attack");

            Transform point = attackPoint != null ? attackPoint : transform;
            if (slashVFXPrefab != null)
            {
                GameObject slash = Instantiate(slashVFXPrefab, point.position, point.rotation);
                slash.name = "VFX_Crusader_MeleeSlash__Runtime";
                Destroy(slash, 4f);
            }

            if (damageDelay > 0f)
                yield return new WaitForSeconds(damageDelay);

            DealMeleeDamage();

            float remaining = Mathf.Max(0.05f, attackCooldown - damageDelay);
            yield return new WaitForSeconds(remaining * 0.65f);
            attacking = false;
            attackRoutine = null;
        }

        private void DealMeleeDamage()
        {
            Transform point = attackPoint != null ? attackPoint : transform;
            Collider[] hits = Physics.OverlapSphere(
                point.position,
                attackRadius,
                enemyLayers,
                QueryTriggerInteraction.Collide);

            HashSet<SkeletonEnemyHealth> damaged = new HashSet<SkeletonEnemyHealth>();

            foreach (Collider hit in hits)
            {
                if (hit == null) continue;

                SkeletonEnemyHealth enemy = hit.GetComponentInParent<SkeletonEnemyHealth>();
                if (enemy == null || enemy.IsDead || !damaged.Add(enemy))
                    continue;

                enemy.TakeDamage(attackDamage);

                GameObject impact = hitVFXPrefab != null ? hitVFXPrefab : impactVFXPrefab;
                if (impact != null)
                {
                    Vector3 impactPosition = hit.ClosestPoint(point.position);
                    if (impactPosition == Vector3.zero)
                        impactPosition = enemy.transform.position + Vector3.up;

                    GameObject fx = Instantiate(impact, impactPosition, Quaternion.identity);
                    fx.name = "VFX_Crusader_MeleeHit__Runtime";
                    Destroy(fx, 5f);
                }
            }
        }

        private Transform CreateRuntimeAttackPoint()
        {
            GameObject point = new GameObject("02_ATTACK_POINT__MELEE");
            point.transform.SetParent(transform, false);
            point.transform.localPosition = new Vector3(0f, 1.05f, 1.25f);
            return point.transform;
        }

        private void ReadInput()
        {
            if (!gameplayEnabled || attacking)
            {
                moveInput = 0f;
                turnInput = 0f;
                return;
            }

#if ENABLE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                moveInput = 0f;
                turnInput = 0f;
                return;
            }

            moveInput = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                      - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
            turnInput = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                      - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
#else
            moveInput = Input.GetAxisRaw("Vertical");
            turnInput = Input.GetAxisRaw("Horizontal");
#endif
        }

        private void UpdateAnimator()
        {
            float speed = gameplayEnabled && !attacking ? Mathf.Abs(moveInput) : 0f;
            SetSpeedParameter(speed);
        }

        private void SetSpeedParameter(float value)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == SpeedHash && parameter.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat(SpeedHash, value, 0.08f, Time.deltaTime);
                    return;
                }
            }
        }

        private void TryTrigger(int hash, string parameterName)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(hash);
                    return;
                }
            }
        }

        private static bool AttackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            return mouse || space;
#else
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#endif
        }

        private void OnDrawGizmosSelected()
        {
            Transform point = attackPoint != null ? attackPoint : transform;
            Gizmos.color = new Color(1f, 0.2f, 0.08f, 0.9f);
            Gizmos.DrawWireSphere(point.position, attackRadius);
        }
    }
}
