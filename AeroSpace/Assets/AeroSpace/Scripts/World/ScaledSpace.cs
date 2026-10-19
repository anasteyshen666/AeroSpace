using System;
using AeroSpace.Core;
using UnityEngine;

namespace AeroSpace.World
{
    [DefaultExecutionOrder(100)]
    public class ScaledSpace : MonoBehaviour
    {
        public const double Scale = 1000;

        [SerializeField] Camera mainCamera;
        [SerializeField] Camera scaledCamera;
        [SerializeField] MeshFilter planetMesh;
        [SerializeField] Color skyColor = new(0.42f, 0.62f, 0.92f);
        [SerializeField] Color spaceColor = new(0.01f, 0.01f, 0.02f);
        [SerializeField] Color skyAmbient = new(0.45f, 0.5f, 0.58f);
        [SerializeField] Color spaceAmbient = new(0.06f, 0.06f, 0.08f);

        void Start()
        {
            var planet = Planet.Instance;
            planetMesh.sharedMesh = PlanetMesh.Sphere((float)(planet.Radius / Scale), 256, 128);
            planetMesh.transform.SetPositionAndRotation(transform.position, planet.Orientation);
        }

        void OnDestroy()
        {
            if (planetMesh && planetMesh.sharedMesh)
                Destroy(planetMesh.sharedMesh);
        }

        void LateUpdate()
        {
            var planet = Planet.Instance;
            var cameraWorld = FloatingOrigin.ToWorld(mainCamera.transform.position);
            scaledCamera.transform.SetPositionAndRotation(
                transform.position + (Vector3)(cameraWorld / Scale), mainCamera.transform.rotation);
            scaledCamera.fieldOfView = mainCamera.fieldOfView;

            var atmosphere = planet.Atmosphere;
            double density = atmosphere.Density(planet.Altitude(cameraWorld)) / atmosphere.seaLevelDensity;
            float air = (float)Math.Sqrt(density);
            scaledCamera.backgroundColor = Color.Lerp(spaceColor, skyColor, air);
            RenderSettings.ambientLight = Color.Lerp(spaceAmbient, skyAmbient, air);
        }
    }
}
