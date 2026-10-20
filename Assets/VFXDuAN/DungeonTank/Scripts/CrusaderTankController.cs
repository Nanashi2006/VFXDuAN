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
        [Min(0.1f)] public float moveSpeed = 7f;
        [Min(1f)] public float turnSpeed = 95f;

        [Header("Weapon")]
        public Transform muzzle;
        [Min(0.05f)] public float fireCooldown = 0.45f;
        [Min(1f)] public float projectileSpeed = 24f;
        [Min(1f)] public float projectileDamage = 25f;
        public GameObject muzzleVFXPrefab;
        public GameObject impactVFXPrefab;

        private Rigidbody body;
        private float moveInput;
        private float turnInput;
        private float nextFireTime;
        private bool gameplayEnabled = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Update()
        {
            ReadInput();

            if (gameplayEnabled && FirePressed() && Time.time >= nextFireTime)
                Fire();
        }

        private void FixedUpdate()
        {
            if (!gameplayEnabled) return;

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
            }
        }

        private void Fire()
        {
            nextFireTime = Time.time + fireCooldown;

            Transform source = muzzle != null ? muzzle : transform;
            Vector3 spawnPos = source.position + source.forward * 0.35f;
            Quaternion spawnRot = Quaternion.LookRotation(source.forward, Vector3.up);

            TankProjectile projectile = TankProjectile.Create(
                spawnPos,
                spawnRot,
                projectileSpeed,
                projectileDamage,
                impactVFXPrefab,
                gameObject);

            if (projectile != null)
                projectile.name = "TankShell__Runtime";

            if (muzzleVFXPrefab != null)
            {
                GameObject fx = Instantiate(muzzleVFXPrefab, spawnPos, spawnRot);
                fx.name = "VFX_Tank_MuzzleFlash__Runtime";
                Destroy(fx, 5f);
            }
        }

        private void ReadInput()
        {
            if (!gameplayEnabled)
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

        private static bool FirePressed()
        {
#if ENABLE_INPUT_SYSTEM
            bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            return mouse || space;
#else
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#endif
        }
    }
}
