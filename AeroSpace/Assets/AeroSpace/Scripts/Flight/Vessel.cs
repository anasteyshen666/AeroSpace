using System;
using System.Collections.Generic;
using System.Text;
using AeroSpace.Core;
using AeroSpace.World;
using UnityEngine;

namespace AeroSpace.Flight
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vessel : MonoBehaviour
    {
        [SerializeField] float frontDragCoefficient = 0.4f;
        [SerializeField] float sideDragCoefficient = 1.0f;
        [SerializeField] float debrisRange = 20_000f;

        Rigidbody body;
        Part[] parts;
        Engine[] engines;
        int[] engineSegments;
        FuelTank[] tanks;
        int[] tankSegments;
        Parachute[] parachutes;
        bool[] activeSegments;
        float frontalArea, sideArea, wheelTorque;
        const float DebrisTumble = 0.15f;

        float throttle = 1f;
        bool touching;
        int pendingStages;

        public Rigidbody Body => body;
        public IReadOnlyList<Part> Parts => parts;
        public bool IsDebris { get; private set; }
        public bool Landed { get; private set; }
        public float Throttle { get => throttle; set => throttle = Mathf.Clamp01(value); }
        public int NextStage { get; private set; }
        public int StageCount { get; private set; }
        public float PitchYawTorque { get; private set; }
        public float RollTorque { get; private set; }

        public double Altitude { get; private set; }
        public float VerticalSpeed { get; private set; }
        public float Speed { get; private set; }
        public float Thrust { get; private set; }
        public float Twr { get; private set; }
        public float DynamicPressure { get; private set; }
        public float StageFuel { get; private set; }
        public float StageFuelCapacity { get; private set; }
        public bool Flameout { get; private set; }
        public Vector3d WorldPosition => FloatingOrigin.ToWorld(body.worldCenterOfMass);

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 0f;
            body.angularDamping = 0.05f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            RefreshParts();
        }

        public void RefreshParts()
        {
            parts = GetComponentsInChildren<Part>();
            Array.Sort(parts, (a, b) => LocalHeight(b).CompareTo(LocalHeight(a)));

            int segment = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0 && parts[i].TryGetComponent<Decoupler>(out _))
                    segment++;
                parts[i].Segment = segment;
            }
            activeSegments = new bool[segment + 1];

            var engineList = new List<Engine>();
            var engineSegmentList = new List<int>();
            var tankList = new List<FuelTank>();
            var tankSegmentList = new List<int>();
            var chuteList = new List<Parachute>();
            wheelTorque = 0f;
            frontalArea = 0f;
            sideArea = 0f;
            StageCount = 0;
            foreach (var part in parts)
            {
                if (part.TryGetComponent<Engine>(out var engine))
                {
                    engineList.Add(engine);
                    engineSegmentList.Add(part.Segment);
                }
                if (part.TryGetComponent<FuelTank>(out var tank))
                {
                    tankList.Add(tank);
                    tankSegmentList.Add(part.Segment);
                }
                if (part.TryGetComponent<Parachute>(out var chute))
                    chuteList.Add(chute);
                if (part.TryGetComponent<ReactionWheel>(out var wheel))
                    wheelTorque += wheel.Torque;

                frontalArea = Mathf.Max(frontalArea, Mathf.PI * part.Diameter * part.Diameter * 0.25f);
                sideArea += part.Diameter * part.Length;
                StageCount = Mathf.Max(StageCount, part.Stage + 1);
            }
            engines = engineList.ToArray();
            engineSegments = engineSegmentList.ToArray();
            tanks = tankList.ToArray();
            tankSegments = tankSegmentList.ToArray();
            parachutes = chuteList.ToArray();
            UpdateMass();
        }

        public void ActivateNextStage()
        {
            if (!IsDebris)
                pendingStages++;
        }

        void RunStage()
        {
            if (NextStage >= StageCount)
                return;
            int stage = NextStage++;
            var snapshot = parts;
            foreach (var part in snapshot)
            {
                if (part.Stage != stage)
                    continue;
                if (part.TryGetComponent<Engine>(out var engine))
                    engine.Ignited = true;
                if (part.TryGetComponent<Parachute>(out var chute))
                    chute.Arm();
            }
            foreach (var part in snapshot)
                if (part.Stage == stage && part.TryGetComponent<Decoupler>(out var decoupler))
                    Decouple(part, decoupler);
        }

        public string DescribeStage(int stage)
        {
            var text = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Stage != stage)
                    continue;
                if (text.Length > 0)
                    text.Append(", ");
                text.Append(part.Definition.displayName);
            }
            return text.ToString();
        }

        void Decouple(Part decouplerPart, Decoupler decoupler)
        {
            transform.SetPositionAndRotation(body.position, body.rotation);
            float splitHeight = LocalHeight(decouplerPart) + 0.001f;
            var debrisObject = new GameObject(name + " Debris");
            debrisObject.transform.SetPositionAndRotation(body.position, body.rotation);
            debrisObject.AddComponent<Rigidbody>();
            foreach (var part in parts)
                if (LocalHeight(part) <= splitHeight)
                    part.transform.SetParent(debrisObject.transform, true);

            var debris = debrisObject.AddComponent<Vessel>();
            debris.MakeDebris();
            RefreshParts();

            debris.body.linearVelocity = body.GetPointVelocity(debris.body.worldCenterOfMass);
            debris.body.angularVelocity = body.angularVelocity + UnityEngine.Random.insideUnitSphere * DebrisTumble;
            IgnoreCollisions(debris);

            var axis = decouplerPart.transform.up;
            body.AddForce(axis * decoupler.EjectionImpulse, ForceMode.Impulse);
            debris.body.AddForce(-axis * decoupler.EjectionImpulse, ForceMode.Impulse);
            decoupler.Fire();
        }

        void MakeDebris()
        {
            IsDebris = true;
            NextStage = StageCount;
            foreach (var engine in engines)
                engine.Ignited = false;
        }

        void IgnoreCollisions(Vessel other)
        {
            var mine = GetComponentsInChildren<Collider>();
            var theirs = other.GetComponentsInChildren<Collider>();
            foreach (var a in mine)
                foreach (var b in theirs)
                    Physics.IgnoreCollision(a, b);
        }

        float LocalHeight(Part part) => transform.InverseTransformPoint(part.transform.position).y;

        float DrawFuel(int segment, float amount)
        {
            float drawn = 0f;
            for (int i = 0; i < tanks.Length && drawn < amount; i++)
                if (tankSegments[i] == segment)
                    drawn += tanks[i].Drain(amount - drawn);
            return drawn;
        }

        void OnCollisionStay(Collision collision) => touching = true;

        void FixedUpdate()
        {
            var planet = Planet.Instance;
            Landed = touching && body.linearVelocity.sqrMagnitude < 4f;
            touching = false;
            for (; pendingStages > 0; pendingStages--)
                RunStage();
            UpdateMass();

            var world = WorldPosition;
            Altitude = planet.Altitude(world);
            if (IsDebris && (Altitude < -10 || OutOfRange()))
            {
                Destroy(gameObject);
                return;
            }

            var gravity = planet.Gravity(world);
            body.AddForce((Vector3)gravity, ForceMode.Acceleration);

            var atmosphere = planet.Atmosphere;
            float pressureRatio = (float)atmosphere.PressureRatio(Altitude);
            float density = (float)atmosphere.Density(Altitude);
            var airVelocity = body.linearVelocity - planet.WindAt(world);
            ApplyDrag(density, airVelocity);
            foreach (var chute in parachutes)
                chute.Simulate(body, Altitude, pressureRatio, density, airVelocity, Landed, Time.fixedDeltaTime);
            ApplyThrust(pressureRatio);
            UpdateControlAuthority();
            UpdateStageFuel();

            var velocity = body.linearVelocity;
            Speed = velocity.magnitude;
            VerticalSpeed = Vector3.Dot(velocity, (Vector3)world.normalized);
            Twr = Thrust / (body.mass * (float)gravity.magnitude);
        }

        bool OutOfRange()
        {
            var focus = FloatingOrigin.Focus;
            return focus && (transform.position - focus.position).sqrMagnitude > debrisRange * debrisRange;
        }

        void UpdateMass()
        {
            float mass = 0f;
            var moment = Vector3.zero;
            foreach (var part in parts)
            {
                float m = part.Mass;
                mass += m;
                moment += m * transform.InverseTransformPoint(part.CenterOfMass);
            }
            body.mass = mass;
            body.centerOfMass = moment / mass;
        }

        void ApplyDrag(float density, Vector3 airVelocity)
        {
            float speed = airVelocity.magnitude;
            DynamicPressure = 0.5f * density * speed * speed;
            if (speed < 0.01f)
                return;

            var direction = airVelocity / speed;
            float cos = Vector3.Dot(transform.up, direction);
            float cos2 = cos * cos;
            float dragArea = frontDragCoefficient * frontalArea * cos2 + sideDragCoefficient * sideArea * (1f - cos2);
            body.AddForce(-direction * (DynamicPressure * dragArea));
        }

        void ApplyThrust(float pressureRatio)
        {
            float dt = Time.fixedDeltaTime;
            Thrust = 0f;
            Flameout = false;
            for (int i = 0; i < engines.Length; i++)
            {
                var engine = engines[i];
                float required = engine.FuelRequired(throttle, dt);
                float fraction = required > 0f ? DrawFuel(engineSegments[i], required) / required : 0f;
                engine.Burn(throttle, pressureRatio, fraction);
                Flameout |= engine.Flameout;
                if (engine.CurrentThrust <= 0f)
                    continue;
                body.AddForceAtPosition(engine.transform.up * engine.CurrentThrust, engine.transform.position);
                Thrust += engine.CurrentThrust;
            }
        }

        void UpdateControlAuthority()
        {
            float gimbal = 0f;
            var center = body.worldCenterOfMass;
            var axis = transform.up;
            foreach (var engine in engines)
            {
                if (engine.CurrentThrust <= 0f)
                    continue;
                float lever = Mathf.Abs(Vector3.Dot(engine.transform.position - center, axis));
                gimbal += engine.CurrentThrust * Mathf.Sin(engine.GimbalRange * Mathf.Deg2Rad) * lever;
            }
            PitchYawTorque = wheelTorque + gimbal;
            RollTorque = wheelTorque;
        }

        void UpdateStageFuel()
        {
            Array.Clear(activeSegments, 0, activeSegments.Length);
            bool any = false;
            for (int i = 0; i < engines.Length; i++)
            {
                if (!engines[i].Ignited)
                    continue;
                activeSegments[engineSegments[i]] = true;
                any = true;
            }
            if (!any)
                activeSegments[activeSegments.Length - 1] = true;

            float fuel = 0f, capacity = 0f;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (!activeSegments[tankSegments[i]])
                    continue;
                fuel += tanks[i].Fuel;
                capacity += tanks[i].Capacity;
            }
            StageFuel = fuel;
            StageFuelCapacity = capacity;
        }
    }
}
