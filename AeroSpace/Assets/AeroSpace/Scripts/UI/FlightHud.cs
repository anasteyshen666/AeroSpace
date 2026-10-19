using System.Globalization;
using System.Text;
using AeroSpace.Flight;
using TMPro;
using UnityEngine;

namespace AeroSpace.UI
{
    public class FlightHud : MonoBehaviour
    {
        [SerializeField] Vessel vessel;
        [SerializeField] AttitudeControl attitude;
        [SerializeField] TMP_Text telemetry;

        readonly StringBuilder text = new();
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        void Update()
        {
            text.Clear();
            text.Append("<mspace=0.6em>");
            Line("ALTITUDE", Distance(vessel.Altitude));
            Line("VERT SPEED", vessel.VerticalSpeed.ToString("F1", Invariant) + " m/s");
            Line("SPEED", vessel.Speed.ToString("F1", Invariant) + " m/s");
            text.Append('\n');
            Line("THROTTLE", (vessel.Throttle * 100f).ToString("F0", Invariant) + " %");
            Line("THRUST", (vessel.Thrust / 1000f).ToString("F1", Invariant) + " kN");
            Line("TWR", vessel.Twr.ToString("F2", Invariant));
            float fuel = vessel.FuelCapacity > 0f ? vessel.Fuel / vessel.FuelCapacity : 0f;
            Line("FUEL", (fuel * 100f).ToString("F1", Invariant) + " %");
            Line("MASS", (vessel.Body.mass / 1000f).ToString("F2", Invariant) + " t");
            Line("SAS", attitude.SasEnabled ? "ON" : "OFF");

            if (!vessel.EnginesIgnited)
                text.Append("\n<color=#FFD25A>Press SPACE to ignite</color>");
            else if (vessel.Flameout)
                text.Append("\n<color=#FF6A50>OUT OF FUEL</color>");

            telemetry.SetText(text);
        }

        void Line(string label, string value) => text.Append(label.PadRight(12)).Append(value).Append('\n');

        static string Distance(double meters) =>
            meters < 10_000
                ? meters.ToString("F0", Invariant) + " m"
                : (meters / 1000).ToString("F2", Invariant) + " km";
    }
}
