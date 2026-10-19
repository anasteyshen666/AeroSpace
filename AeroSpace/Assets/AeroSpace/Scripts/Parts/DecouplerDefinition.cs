using AeroSpace.Flight;
using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Parts/Decoupler")]
    public class DecouplerDefinition : PartDefinition
    {
        public float ejectionImpulse = 10_000f;
        public Material smokeMaterial;

        public override void AddModules(GameObject part) =>
            part.AddComponent<Decoupler>().Configure(ejectionImpulse, smokeMaterial, Length, Diameter * 0.5f);
    }
}
