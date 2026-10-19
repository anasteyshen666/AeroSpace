using AeroSpace.Core;
using AeroSpace.Flight;
using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Parts/Engine")]
    public class EngineDefinition : PartDefinition
    {
        public float vacuumThrust = 100_000f;
        public float vacuumIsp = 300f;
        public float seaLevelIsp = 250f;
        public float gimbalRange = 3f;
        public Material flameMaterial;

        public override void AddModules(GameObject part)
        {
            var engine = part.AddComponent<Engine>();
            engine.Configure(vacuumThrust, vacuumIsp, seaLevelIsp, gimbalRange);

            float nozzleRadius = sections.Count > 0 ? sections[0].bottomRadius : 0.5f;
            var flame = new GameObject("Flame");
            flame.transform.SetParent(part.transform, false);
            flame.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            var flameRenderer = flame.AddComponent<MeshRenderer>();
            flameRenderer.sharedMaterial = flameMaterial;
            flameRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            flame.AddComponent<FrustumShape>().Configure(nozzleRadius * 0.85f, nozzleRadius * 0.12f, 1f, 16);
            flame.AddComponent<EngineFlame>().Configure(engine, nozzleRadius * 9f);
        }
    }
}
