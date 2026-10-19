using AeroSpace.Core;
using UnityEngine;

namespace AeroSpace.World
{
    [DefaultExecutionOrder(-500), RequireComponent(typeof(BoxCollider))]
    public class GroundCollider : MonoBehaviour
    {
        [SerializeField] float maxAltitude = 3_000f;

        BoxCollider box;

        void Awake() => box = GetComponent<BoxCollider>();

        void FixedUpdate()
        {
            var focus = FloatingOrigin.Focus;
            if (!focus)
                return;
            var planet = Planet.Instance;
            var focusWorld = FloatingOrigin.ToWorld(focus.position);
            box.enabled = planet.Altitude(focusWorld) < maxAltitude;
            if (!box.enabled)
                return;

            var up = focusWorld.normalized;
            transform.SetPositionAndRotation(
                FloatingOrigin.ToLocal(up * planet.Radius),
                Quaternion.FromToRotation(Vector3.up, (Vector3)up));
        }
    }
}
