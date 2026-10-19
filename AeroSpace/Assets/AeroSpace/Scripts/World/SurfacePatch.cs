using AeroSpace.Core;
using UnityEngine;

namespace AeroSpace.World
{
    [DefaultExecutionOrder(60), RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SurfacePatch : MonoBehaviour
    {
        [SerializeField] Transform focus;
        [SerializeField] float patchRadius = 30_000f;
        [SerializeField] float rebuildDistance = 2_000f;
        [SerializeField] float maxAltitude = 60_000f;
        [SerializeField] int rings = 64;
        [SerializeField] int segments = 96;

        Mesh mesh;
        MeshRenderer meshRenderer;
        Vector3d center;
        bool built;

        void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            mesh = new Mesh { name = "SurfacePatch" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        void OnDestroy() => Destroy(mesh);

        void LateUpdate()
        {
            var planet = Planet.Instance;
            var focusWorld = FloatingOrigin.ToWorld(focus.position);
            bool visible = planet.Altitude(focusWorld) < maxAltitude;
            meshRenderer.enabled = visible;
            if (!visible)
                return;

            var surface = focusWorld.normalized * planet.Radius;
            if (!built || (surface - center).magnitude > rebuildDistance)
            {
                center = surface;
                PlanetMesh.Patch(mesh, planet, center, patchRadius, rings, segments);
                built = true;
            }
            transform.SetPositionAndRotation(FloatingOrigin.ToLocal(center), Quaternion.identity);
        }
    }
}
