using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeroSpace.Parts
{
    public enum PartCategory { Pod, Tank, Engine, Decoupler, HeatShield, Parachute }

    [Serializable]
    public struct ShapeSection
    {
        public float bottomRadius;
        public float topRadius;
        public float height;
        public Material material;
    }

    [CreateAssetMenu(menuName = "AeroSpace/Parts/Generic Part")]
    public class PartDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public PartCategory category;
        public float dryMass = 100f;
        public List<ShapeSection> sections = new();

        public float Length
        {
            get
            {
                float length = 0f;
                foreach (var section in sections)
                    length += section.height;
                return length;
            }
        }

        public float Diameter
        {
            get
            {
                float radius = 0f;
                foreach (var section in sections)
                    radius = Mathf.Max(radius, section.bottomRadius, section.topRadius);
                return radius * 2f;
            }
        }

        public virtual void AddModules(GameObject part) { }
    }
}
