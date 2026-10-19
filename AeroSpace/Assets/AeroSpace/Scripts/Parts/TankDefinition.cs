using AeroSpace.Flight;
using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Parts/Tank")]
    public class TankDefinition : PartDefinition
    {
        public float fuelCapacity = 1000f;

        public override void AddModules(GameObject part) =>
            part.AddComponent<FuelTank>().Configure(fuelCapacity);
    }
}
