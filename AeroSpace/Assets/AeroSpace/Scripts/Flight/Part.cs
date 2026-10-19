using AeroSpace.Parts;
using UnityEngine;

namespace AeroSpace.Flight
{
    public class Part : MonoBehaviour
    {
        FuelTank tank;

        public PartDefinition Definition { get; private set; }
        public int Stage { get; private set; } = -1;
        public int Segment { get; set; }

        public float Mass => Definition.dryMass + (tank ? tank.Fuel : 0f);
        public float Diameter => Definition.Diameter;
        public float Length => Definition.Length;
        public Vector3 CenterOfMass => transform.TransformPoint(0f, Length * 0.5f, 0f);

        public void Configure(PartDefinition definition, int stage)
        {
            Definition = definition;
            Stage = stage;
            TryGetComponent(out tank);
        }
    }
}
