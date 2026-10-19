using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Vessel Design")]
    public class VesselDesignAsset : ScriptableObject
    {
        public VesselDesign design = new();
    }
}
