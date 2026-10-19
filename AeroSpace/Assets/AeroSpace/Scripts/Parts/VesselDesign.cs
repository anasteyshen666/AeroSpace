using System;
using System.Collections.Generic;

namespace AeroSpace.Parts
{
    [Serializable]
    public class DesignPart
    {
        public PartDefinition part;
        public int stage = -1;
    }

    [Serializable]
    public class VesselDesign
    {
        public string name = "Untitled";
        public List<DesignPart> parts = new();

        public void AutoStage()
        {
            int engineStage = 0, lastStage = -1;
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                var entry = parts[i];
                entry.stage = entry.part.category switch
                {
                    PartCategory.Engine => engineStage,
                    PartCategory.Decoupler => ++engineStage,
                    _ => -1,
                };
                lastStage = Math.Max(lastStage, entry.stage);
            }
            foreach (var entry in parts)
                if (entry.part.category == PartCategory.Parachute)
                    entry.stage = lastStage + 1;
        }
    }
}
