using UnityEngine;

namespace AeroSpace.Flight
{
    public class FuelTank : MonoBehaviour
    {
        [SerializeField] float capacity = 4000f;
        [SerializeField] float fuel = 4000f;

        public float Capacity => capacity;
        public float Fuel => fuel;

        public float Drain(float amount)
        {
            float taken = Mathf.Min(amount, fuel);
            fuel -= taken;
            return taken;
        }
    }
}
