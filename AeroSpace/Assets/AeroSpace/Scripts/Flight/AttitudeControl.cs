using UnityEngine;

namespace AeroSpace.Flight
{
    [RequireComponent(typeof(Vessel))]
    public class AttitudeControl : MonoBehaviour
    {
        [SerializeField] float maxTurnRate = 0.35f;
        [SerializeField] float rateGain = 3f;
        [SerializeField] float holdGain = 4f;
        [SerializeField] float dampingGain = 4f;
        [SerializeField] float holdCaptureRate = 0.02f;

        Vessel vessel;
        Quaternion target;
        bool holding;

        public Vector3 Input { get; set; }
        public bool SasEnabled { get; set; } = true;

        void Awake() => vessel = GetComponent<Vessel>();

        void FixedUpdate()
        {
            float pitchYaw = vessel.PitchYawTorque, roll = vessel.RollTorque;
            if (pitchYaw <= 0f && roll <= 0f)
                return;

            var body = vessel.Body;
            var omega = body.angularVelocity;
            Vector3 accel;
            if (Input.sqrMagnitude > 1e-4f)
            {
                holding = false;
                var desired = transform.TransformDirection(new Vector3(Input.x, -Input.z, -Input.y) * maxTurnRate);
                accel = rateGain * (desired - omega);
            }
            else if (SasEnabled)
            {
                if (!holding && omega.magnitude < holdCaptureRate)
                {
                    target = body.rotation;
                    holding = true;
                }
                accel = -dampingGain * omega;
                if (holding)
                    accel += holdGain * RotationError(target, body.rotation);
            }
            else
            {
                holding = false;
                return;
            }

            var localTorque = transform.InverseTransformDirection(ApplyInertia(body, accel));
            localTorque = new Vector3(
                Mathf.Clamp(localTorque.x, -pitchYaw, pitchYaw),
                Mathf.Clamp(localTorque.y, -roll, roll),
                Mathf.Clamp(localTorque.z, -pitchYaw, pitchYaw));
            body.AddTorque(transform.TransformDirection(localTorque));
        }

        static Vector3 RotationError(Quaternion target, Quaternion current)
        {
            (target * Quaternion.Inverse(current)).ToAngleAxis(out float angle, out var axis);
            if (angle > 180f)
                angle -= 360f;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x))
                return Vector3.zero;
            return axis * (angle * Mathf.Deg2Rad);
        }

        static Vector3 ApplyInertia(Rigidbody body, Vector3 worldAccel)
        {
            var frame = body.rotation * body.inertiaTensorRotation;
            var local = Quaternion.Inverse(frame) * worldAccel;
            local.Scale(body.inertiaTensor);
            return frame * local;
        }
    }
}
