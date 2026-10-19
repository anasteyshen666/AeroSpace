using UnityEngine;

namespace AeroSpace.Flight
{
    public class Engine : MonoBehaviour
    {
        public const float G0 = 9.80665f;

        [SerializeField] float vacuumThrust = 150_000f;
        [SerializeField] float vacuumIsp = 300f;
        [SerializeField] float seaLevelIsp = 250f;

        public bool Ignited { get; set; }
        public bool Flameout { get; private set; }
        public float CurrentThrust { get; private set; }
        public float VacuumThrust => vacuumThrust;
        public float MaxFuelFlow => vacuumThrust / (vacuumIsp * G0);

        public float Isp(float pressureRatio) => Mathf.Lerp(vacuumIsp, seaLevelIsp, pressureRatio);
        public float MaxThrust(float pressureRatio) => MaxFuelFlow * Isp(pressureRatio) * G0;

        public float FuelRequired(float throttle, float dt) => Ignited ? throttle * MaxFuelFlow * dt : 0f;

        public void Burn(float throttle, float pressureRatio, float fuelFraction)
        {
            Flameout = Ignited && throttle > 0f && fuelFraction <= 0f;
            CurrentThrust = Ignited ? throttle * MaxThrust(pressureRatio) * fuelFraction : 0f;
        }
    }
}
