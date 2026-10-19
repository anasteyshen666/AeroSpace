using UnityEngine;

namespace AeroSpace.Flight
{
    public class ReactionWheel : MonoBehaviour
    {
        public float Torque { get; private set; }

        public void Configure(float torque) => Torque = torque;
    }
}
