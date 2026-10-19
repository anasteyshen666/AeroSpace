using System.Globalization;
using System.Text;
using AeroSpace.Flight;
using TMPro;
using UnityEngine;

namespace AeroSpace.UI
{
    public class FlightHud : MonoBehaviour
    {
        [SerializeField] TMP_Text telemetry;

        readonly StringBuilder text = new();
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        void Update()
        {
            var vessel = FlightSession.Instance.ActiveVessel;
            var attitude = vessel.GetComponent<AttitudeControl>();

            text.Clear();
            text.Append("<mspace=0.6em>");
            Line("ALTITUDE", Distance(vessel.Altitude));
            Line("VERT SPEED", vessel.VerticalSpeed.ToString("F1", Invariant) + " m/s");
            Line("SPEED", vessel.Speed.ToString("F1", Invariant) + " m/s");
            text.Append('\n');
            Line("THROTTLE", (vessel.Throttle * 100f).ToString("F0", Invariant) + " %");
            Line("THRUST", (vessel.Thrust / 1000f).ToString("F1", Invariant) + " kN");
            Line("TWR", vessel.Twr.ToString("F2", Invariant));
            float fuel = vessel.StageFuelCapacity > 0f ? vessel.StageFuel / vessel.StageFuelCapacity : 0f;
            Line("STAGE FUEL", (fuel * 100f).ToString("F1", Invariant) + " %");
            Line("MASS", (vessel.Body.mass / 1000f).ToString("F2", Invariant) + " t");
            Line("SAS", attitude.SasEnabled ? "ON" : "OFF");
            text.Append('\n');
            Line("STAGE", vessel.NextStage + " / " + vessel.StageCount);

            if (vessel.NextStage < vessel.StageCount)
            {
                string action = vessel.NextStage == 0 ? "launch" : "next stage";
                text.Append("</mspace>\n<color=#FFD25A>SPACE: ").Append(action).Append("</color>\n<size=80%>")
                    .Append(vessel.DescribeStage(vessel.NextStage)).Append("</size>");
            }
            if (vessel.Flameout)
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
