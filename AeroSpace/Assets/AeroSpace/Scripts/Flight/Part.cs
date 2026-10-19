using UnityEngine;

namespace AeroSpace.Flight
{
    public class Part : MonoBehaviour
    {
        [SerializeField] float dryMass = 500f;
        [SerializeField] float diameter = 2f;
        [SerializeField] float length = 2f;

        FuelTank tank;

        public float Mass => dryMass + (tank ? tank.Fuel : 0f);
        public float Diameter => diameter;
        public float Length => length;
        public Vector3 CenterOfMass => transform.TransformPoint(0f, length * 0.5f, 0f);

        void Awake() => TryGetComponent(out tank);
    }
}
