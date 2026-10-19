using AeroSpace.Core;
using AeroSpace.Parts;
using UnityEngine;

namespace AeroSpace.Flight
{
    [DefaultExecutionOrder(-1500)]
    public class FlightSession : MonoBehaviour
    {
        public static FlightSession Instance { get; private set; }
        public static VesselDesign PendingDesign { get; set; }

        [SerializeField] VesselDesignAsset defaultDesign;
        [SerializeField] Transform launchPoint;

        public Vessel ActiveVessel { get; private set; }

        void Awake()
        {
            Instance = this;
            var design = PendingDesign ?? defaultDesign.design;
            ActiveVessel = VesselBuilder.Build(design, launchPoint.position, launchPoint.rotation);
            ActiveVessel.gameObject.AddComponent<VesselInput>();
            FloatingOrigin.Focus = ActiveVessel.transform;
        }
    }
}
