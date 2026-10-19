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
        [SerializeField, Range(0f, 1f)] float throttle = 1f;

        Rigidbody body;
        Part[] parts;
        Engine[] engines;
        FuelTank[] tanks;
        float frontalArea;
        float sideArea;

        public Rigidbody Body => body;
        public float Throttle { get => throttle; set => throttle = Mathf.Clamp01(value); }
        public float MaxTorque { get; private set; }
        public bool EnginesIgnited { get; private set; }

        public double Altitude { get; private set; }
        public float VerticalSpeed { get; private set; }
        public float Speed { get; private set; }
        public float Thrust { get; private set; }
        public float Twr { get; private set; }
        public float DynamicPressure { get; private set; }
        public float Fuel { get; private set; }
        public float FuelCapacity { get; private set; }
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
            engines = GetComponentsInChildren<Engine>();
            tanks = GetComponentsInChildren<FuelTank>();

            MaxTorque = 0f;
            foreach (var wheel in GetComponentsInChildren<ReactionWheel>())
                MaxTorque += wheel.Torque;

            frontalArea = 0f;
            sideArea = 0f;
            foreach (var part in parts)
            {
                frontalArea = Mathf.Max(frontalArea, Mathf.PI * part.Diameter * part.Diameter * 0.25f);
                sideArea += part.Diameter * part.Length;
            }
        }

        public void IgniteEngines()
        {
            EnginesIgnited = true;
            foreach (var engine in engines)
                engine.Ignited = true;
        }

        public float DrawFuel(float amount)
        {
            float drawn = 0f;
            foreach (var tank in tanks)
            {
                if (drawn >= amount)
                    break;
                drawn += tank.Drain(amount - drawn);
            }
            return drawn;
        }

        void FixedUpdate()
        {
            var planet = Planet.Instance;
            UpdateMass();

            var world = WorldPosition;
            Altitude = planet.Altitude(world);
            var gravity = planet.Gravity(world);
            body.AddForce((Vector3)gravity, ForceMode.Acceleration);

            var atmosphere = planet.Atmosphere;
            float pressureRatio = (float)atmosphere.PressureRatio(Altitude);
            ApplyDrag((float)atmosphere.Density(Altitude), planet.WindAt(world));
            ApplyThrust(pressureRatio);

            var velocity = body.linearVelocity;
            Speed = velocity.magnitude;
            VerticalSpeed = Vector3.Dot(velocity, (Vector3)world.normalized);
            Twr = Thrust / (body.mass * (float)gravity.magnitude);
        }

        void UpdateMass()
        {
            float mass = 0f, fuel = 0f, capacity = 0f;
            var moment = Vector3.zero;
            foreach (var part in parts)
            {
                float m = part.Mass;
                mass += m;
                moment += m * transform.InverseTransformPoint(part.CenterOfMass);
            }
            foreach (var tank in tanks)
            {
                fuel += tank.Fuel;
                capacity += tank.Capacity;
            }
            body.mass = mass;
            body.centerOfMass = moment / mass;
            Fuel = fuel;
            FuelCapacity = capacity;
        }

        void ApplyDrag(float density, Vector3 wind)
        {
            var airVelocity = body.linearVelocity - wind;
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
            foreach (var engine in engines)
            {
                float required = engine.FuelRequired(throttle, dt);
                float fraction = required > 0f ? DrawFuel(required) / required : 0f;
                engine.Burn(throttle, pressureRatio, fraction);
                Flameout |= engine.Flameout;
                if (engine.CurrentThrust <= 0f)
                    continue;
                body.AddForceAtPosition(engine.transform.up * engine.CurrentThrust, engine.transform.position);
                Thrust += engine.CurrentThrust;
            }
        }
    }
}
