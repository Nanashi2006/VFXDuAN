using UnityEngine;

namespace VFXDuAN.DungeonTank
{
    [DisallowMultipleComponent]
    public class DungeonCameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 7.5f, -10.5f);
        [Min(0.1f)] public float positionSmooth = 6f;
        [Min(0.1f)] public float rotationSmooth = 8f;
        public Vector3 lookOffset = new Vector3(0f, 1.0f, 2.0f);

        private void LateUpdate()
        {
            if (target == null)
            {
                CrusaderTankHealth player = FindAnyObjectByType<CrusaderTankHealth>();
                if (player != null) target = player.transform;
                if (target == null) return;
            }

            Vector3 desired = target.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

            Vector3 lookPoint = target.TransformPoint(lookOffset);
            Vector3 direction = lookPoint - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));
            }
        }
    }
}
