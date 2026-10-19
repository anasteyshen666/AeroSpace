using UnityEngine;

namespace AeroSpace.Flight
{
    public class FuelTank : MonoBehaviour
    {
        public float Capacity { get; private set; }
        public float Fuel { get; private set; }

        public void Configure(float capacity)
        {
            Capacity = capacity;
            Fuel = capacity;
        }

        public float Drain(float amount)
        {
            float taken = Mathf.Min(amount, Fuel);
            Fuel -= taken;
            return taken;
        }
    }
}
