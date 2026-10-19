using UnityEngine;

namespace AeroSpace.Flight
{
    public class ReactionWheel : MonoBehaviour
    {
        [SerializeField] float torque = 15_000f;

        public float Torque => torque;
    }
}
