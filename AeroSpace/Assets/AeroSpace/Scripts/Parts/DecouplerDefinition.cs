using AeroSpace.Flight;
using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Parts/Decoupler")]
    public class DecouplerDefinition : PartDefinition
    {
        public float ejectionImpulse = 2_000f;

        public override void AddModules(GameObject part) =>
            part.AddComponent<Decoupler>().Configure(ejectionImpulse);
    }
}
