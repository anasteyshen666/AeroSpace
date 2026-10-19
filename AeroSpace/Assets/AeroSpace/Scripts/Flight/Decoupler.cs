using UnityEngine;

namespace AeroSpace.Flight
{
    public class Decoupler : MonoBehaviour
    {
        public float EjectionImpulse { get; private set; }

        public void Configure(float impulse) => EjectionImpulse = impulse;
    }
}
