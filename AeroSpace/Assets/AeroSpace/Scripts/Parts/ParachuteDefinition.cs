using AeroSpace.Core;
using AeroSpace.Flight;
using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Parts/Parachute")]
    public class ParachuteDefinition : PartDefinition
    {
        public float semiDeployedArea = 25f;
        public float deployedArea = 700f;
        public float minPressureRatio = 0.04f;
        public float fullDeployAltitude = 1000f;
        public Material canopyMaterial;

        public override void AddModules(GameObject part)
        {
            var canopy = new GameObject("Canopy");
            canopy.transform.SetParent(part.transform, false);
            canopy.AddComponent<MeshRenderer>().sharedMaterial = canopyMaterial;
            canopy.AddComponent<FrustumShape>().Configure(1f, 0.3f, 0.6f, 24);
            canopy.SetActive(false);

            part.AddComponent<Parachute>().Configure(
                semiDeployedArea, deployedArea, minPressureRatio, fullDeployAltitude, Length, canopy.transform);
        }
    }
}
