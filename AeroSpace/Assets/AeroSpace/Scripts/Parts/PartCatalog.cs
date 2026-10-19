using System.Collections.Generic;
using UnityEngine;

namespace AeroSpace.Parts
{
    [CreateAssetMenu(menuName = "AeroSpace/Part Catalog")]
    public class PartCatalog : ScriptableObject
    {
        public List<PartDefinition> parts = new();

        public PartDefinition Find(string id) => parts.Find(part => part.name == id);
    }
}
