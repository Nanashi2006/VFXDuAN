using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class DungeonCameraFollow : MonoBehaviour
    {
        [Header("Third Person Target")]
        public Transform target;
        public Vector3 pivotOffset = new Vector3(0f, 1.65f, 0f);
        public Vector3 lookOffset = new Vector3(0f, 1.25f, 0.75f);

        [Header("Third Person Distance")]
        [Min(1.5f)] public float distance = 6.5f;
        [Min(1.5f)] public float minDistance = 2.8f;
        [Min(2f)] public float maxDistance = 9.5f;
        [Min(0.1f)] public float zoomSpeed = 1.2f;
        public float shoulderOffset = 0.55f;

        [Header("Orbit")]
        [Tooltip("Hold RMB and move the mouse to freely orbit around the Crusader.")]
        public float mouseSensitivity = 0.16f;
        public float defaultPitch = 18f;
        public float minPitch = -12f;
        public float maxPitch = 58f;
        [Min(0.1f)] public float followYawSmooth = 7f;
        [Min(0.1f)] public float positionSmooth = 12f;
        [Min(0.1f)] public float rotationSmooth = 14f;

        [Header("Dungeon Wall Collision")]
        public LayerMask collisionMask = ~0;
        [Min(0.05f)] public float collisionRadius = 0.28f;
        [Min(0f)] public float wallPadding = 0.18f;

        private float yaw;
        private float pitch;
        private bool initialized;
        private bool orbiting;

        private void Start()
        {
            ResolveTarget();
            InitializeOrbit();
        }

        private void OnDisable()
        {
            if (orbiting)
                ReleaseCursor();
        }

        private void LateUpdate()
        {
            if (!ResolveTarget()) return;
            if (!initialized) InitializeOrbit();

            ReadCameraInput();

            if (!orbiting)
            {
                float targetYaw = target.eulerAngles.y;
                yaw = Mathf.LerpAngle(yaw, targetYaw, 1f - Mathf.Exp(-followYawSmooth * Time.deltaTime));
            }

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + pivotOffset;
            Vector3 shoulder = orbitRotation * Vector3.right * shoulderOffset;
            Vector3 desiredCamera = pivot + shoulder - orbitRotation * Vector3.forward * distance;
            Vector3 safeCamera = ResolveCollision(pivot + shoulder, desiredCamera);

            float posT = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, safeCamera, posT);

            Vector3 lookPoint = target.position + lookOffset;
            Vector3 direction = lookPoint - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                float rotT = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotT);
            }
        }

        private bool ResolveTarget()
        {
            if (target != null) return true;

            CrusaderTankHealth player = FindAnyObjectByType<CrusaderTankHealth>();
            if (player != null)
                target = player.transform;

            return target != null;
        }

        private void InitializeOrbit()
        {
            if (target == null) return;

            yaw = target.eulerAngles.y;
            pitch = Mathf.Clamp(defaultPitch, minPitch, maxPitch);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            initialized = true;

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + pivotOffset;
            transform.position = pivot + orbitRotation * Vector3.right * shoulderOffset
                               - orbitRotation * Vector3.forward * distance;
        }

        private void ReadCameraInput()
        {
            bool rmbHeld;
            bool rmbDown;
            bool rmbUp;
            Vector2 mouseDelta;
            float scroll;

#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            rmbHeld = mouse != null && mouse.rightButton.isPressed;
            rmbDown = mouse != null && mouse.rightButton.wasPressedThisFrame;
            rmbUp = mouse != null && mouse.rightButton.wasReleasedThisFrame;
            mouseDelta = mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
            scroll = mouse != null ? mouse.scroll.ReadValue().y / 120f : 0f;
#else
            rmbHeld = Input.GetMouseButton(1);
            rmbDown = Input.GetMouseButtonDown(1);
            rmbUp = Input.GetMouseButtonUp(1);
            mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X") * 10f, Input.GetAxisRaw("Mouse Y") * 10f);
            scroll = Input.mouseScrollDelta.y;
#endif

            if (rmbDown)
            {
                orbiting = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (rmbUp)
            {
                orbiting = false;
                ReleaseCursor();
            }

            if (rmbHeld)
            {
                yaw += mouseDelta.x * mouseSensitivity;
                pitch -= mouseDelta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            if (Mathf.Abs(scroll) > 0.001f)
                distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        private Vector3 ResolveCollision(Vector3 origin, Vector3 desired)
        {
            Vector3 ray = desired - origin;
            float desiredDistance = ray.magnitude;
            if (desiredDistance <= 0.001f) return desired;

            Vector3 direction = ray / desiredDistance;
            RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                collisionRadius,
                direction,
                desiredDistance,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            float closest = desiredDistance;
            bool blocked = false;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;
                Transform hitTransform = hit.collider.transform;
                if (target != null && (hitTransform == target || hitTransform.IsChildOf(target)))
                    continue;

                if (hit.distance < closest)
                {
                    closest = hit.distance;
                    blocked = true;
                }
            }

            if (!blocked) return desired;

            float safeDistance = Mathf.Max(0.15f, closest - wallPadding);
            return origin + direction * safeDistance;
        }

        private static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            Gizmos.DrawWireSphere(target.position + pivotOffset, 0.12f);
        }
    }
}
