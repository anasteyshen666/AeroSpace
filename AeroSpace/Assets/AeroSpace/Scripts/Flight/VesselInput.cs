using UnityEngine;
using UnityEngine.InputSystem;

namespace AeroSpace.Flight
{
    [RequireComponent(typeof(Vessel), typeof(AttitudeControl))]
    public class VesselInput : MonoBehaviour
    {
        [SerializeField] float throttleRate = 0.6f;

        Vessel vessel;
        AttitudeControl attitude;
        InputActionMap map;
        InputAction pitch, yaw, roll, throttle, throttleFull, throttleCut, toggleSas, stage;

        void Awake()
        {
            vessel = GetComponent<Vessel>();
            attitude = GetComponent<AttitudeControl>();
            map = InputSystem.actions.FindActionMap("Flight", true);
            pitch = map.FindAction("Pitch", true);
            yaw = map.FindAction("Yaw", true);
            roll = map.FindAction("Roll", true);
            throttle = map.FindAction("Throttle", true);
            throttleFull = map.FindAction("ThrottleFull", true);
            throttleCut = map.FindAction("ThrottleCut", true);
            toggleSas = map.FindAction("ToggleSas", true);
            stage = map.FindAction("Stage", true);
        }

        void OnEnable() => map.Enable();
        void OnDisable() => map.Disable();

        void Update()
        {
            attitude.Input = new Vector3(pitch.ReadValue<float>(), yaw.ReadValue<float>(), roll.ReadValue<float>());

            float value = vessel.Throttle + throttle.ReadValue<float>() * throttleRate * Time.deltaTime;
            if (throttleFull.WasPressedThisFrame())
                value = 1f;
            if (throttleCut.WasPressedThisFrame())
                value = 0f;
            vessel.Throttle = value;

            if (toggleSas.WasPressedThisFrame())
                attitude.SasEnabled = !attitude.SasEnabled;
            if (stage.WasPressedThisFrame())
                vessel.ActivateNextStage();
        }
    }
}
