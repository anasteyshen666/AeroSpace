using AeroSpace.Flight;
using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Parts/Pod")]
    public class PodDefinition : PartDefinition
    {
        public int crewCapacity = 2;
        public float reactionTorque = 20_000f;

        public override void AddModules(GameObject part) =>
            part.AddComponent<ReactionWheel>().Configure(reactionTorque);
    }
}
