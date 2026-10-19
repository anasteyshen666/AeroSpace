using AeroSpace.Core;
using AeroSpace.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AeroSpace.Flight
{
    [DefaultExecutionOrder(50)]
    public class FlightCamera : MonoBehaviour
    {
        [SerializeField] float distance = 60f;
        [SerializeField] float minDistance = 5f;
        [SerializeField] float maxDistance = 5_000f;
        [SerializeField] float orbitSensitivity = 0.2f;
        [SerializeField] float yaw;
        [SerializeField] float pitch = 10f;
        [SerializeField] float focusSharpness = 3f;

        InputActionMap map;
        InputAction orbit, orbitHold, zoom;
        Vessel tracked;
        Vector3 localFocus;

        void Awake()
        {
            map = InputSystem.actions.FindActionMap("Camera", true);
            orbit = map.FindAction("Orbit", true);
            orbitHold = map.FindAction("OrbitHold", true);
            zoom = map.FindAction("Zoom", true);
        }

        void OnEnable() => map.Enable();
        void OnDisable() => map.Disable();

        void LateUpdate()
        {
            if (orbitHold.IsPressed())
            {
                var delta = orbit.ReadValue<Vector2>() * orbitSensitivity;
                yaw += delta.x;
                pitch = Mathf.Clamp(pitch - delta.y, -89f, 89f);
            }

            float scroll = zoom.ReadValue<float>();
            if (Mathf.Abs(scroll) > 0.01f)
                distance = Mathf.Clamp(distance * (scroll > 0f ? 0.9f : 1.1f), minDistance, maxDistance);

            var target = FlightSession.Instance.ActiveVessel;
            if (target != tracked)
            {
                tracked = target;
                localFocus = target.Body.centerOfMass;
            }
            localFocus = Vector3.Lerp(localFocus, target.Body.centerOfMass, 1f - Mathf.Exp(-focusSharpness * Time.deltaTime));
            var focus = target.transform.TransformPoint(localFocus);
            Planet.Instance.Basis(FloatingOrigin.ToWorld(focus), out var up, out var north, out _);
            var rotation = Quaternion.LookRotation((Vector3)north, (Vector3)up) * Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(focus - rotation * Vector3.forward * distance, rotation);
        }
    }
}
