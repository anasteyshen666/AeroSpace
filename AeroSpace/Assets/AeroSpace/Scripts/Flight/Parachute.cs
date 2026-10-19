using UnityEngine;

namespace AeroSpace.Flight
{
    public class Parachute : MonoBehaviour
    {
        public enum ChuteState { Stowed, Armed, SemiDeployed, Deployed, Cut }

        const float MaxDeploySpeed = 350f;

        float semiArea, fullArea, minPressureRatio, fullDeployAltitude, attachHeight;
        Transform canopy;
        float area;

        public ChuteState State { get; private set; }

        public void Configure(float semi, float full, float minPressure, float fullAltitude, float height, Transform canopyTransform)
        {
            semiArea = semi;
            fullArea = full;
            minPressureRatio = minPressure;
            fullDeployAltitude = fullAltitude;
            attachHeight = height;
            canopy = canopyTransform;
        }

        public void Arm()
        {
            if (State == ChuteState.Stowed)
                State = ChuteState.Armed;
        }

        public void Simulate(Rigidbody body, double altitude, float pressureRatio, float density, Vector3 airVelocity, bool landed, float dt)
        {
            if (landed && State is ChuteState.SemiDeployed or ChuteState.Deployed)
                State = ChuteState.Cut;

            float speed = airVelocity.magnitude;
            if (State == ChuteState.Armed && pressureRatio >= minPressureRatio && speed <= MaxDeploySpeed)
                State = ChuteState.SemiDeployed;
            if (State == ChuteState.SemiDeployed && altitude <= fullDeployAltitude)
                State = ChuteState.Deployed;

            float target = State switch
            {
                ChuteState.Deployed => fullArea,
                ChuteState.SemiDeployed => semiArea,
                _ => 0f,
            };
            float rate = State switch
            {
                ChuteState.Deployed => fullArea / 3f,
                ChuteState.Cut => fullArea,
                _ => semiArea,
            };
            area = Mathf.MoveTowards(area, target, rate * dt);

            var attach = transform.TransformPoint(0f, attachHeight, 0f);
            UpdateCanopy(attach, airVelocity);

            if (area <= 0f || speed < 0.01f)
                return;
            var drag = -airVelocity / speed * (0.5f * density * speed * speed * area);
            body.AddForceAtPosition(drag, attach);
        }

        void UpdateCanopy(Vector3 attach, Vector3 airVelocity)
        {
            bool visible = area > 0.01f;
            canopy.gameObject.SetActive(visible);
            if (!visible)
                return;
            float radius = Mathf.Sqrt(area / Mathf.PI);
            var direction = airVelocity.sqrMagnitude > 1f ? -airVelocity.normalized : transform.up;
            canopy.SetPositionAndRotation(attach + direction * (radius * 1.2f + 2f), Quaternion.FromToRotation(Vector3.up, direction));
            canopy.localScale = Vector3.one * radius;
        }
    }
}
