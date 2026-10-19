using System.Collections.Generic;
using AeroSpace.Core;
using AeroSpace.Parts;
using UnityEngine;

namespace AeroSpace.Flight
{
    public static class VesselBuilder
    {
        public static Vessel Build(VesselDesign design, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject(design.name);
            root.transform.SetPositionAndRotation(position, rotation);
            root.AddComponent<Rigidbody>();

            float height = 0f;
            for (int i = design.parts.Count - 1; i >= 0; i--)
            {
                var entry = design.parts[i];
                var definition = entry.part;
                var part = new GameObject(definition.displayName);
                part.transform.SetParent(root.transform, false);
                part.transform.localPosition = new Vector3(0f, height, 0f);
                BuildShape(part.transform, definition.sections, true);
                definition.AddModules(part);
                part.AddComponent<Part>().Configure(definition, entry.stage);
                height += definition.Length;
            }

            var vessel = root.AddComponent<Vessel>();
            root.AddComponent<AttitudeControl>();
            return vessel;
        }

        public static void BuildShape(Transform parent, IReadOnlyList<ShapeSection> sections, bool colliders)
        {
            float height = 0f;
            foreach (var section in sections)
            {
                var shape = new GameObject("Shape");
                shape.transform.SetParent(parent, false);
                shape.transform.localPosition = new Vector3(0f, height, 0f);
                shape.AddComponent<MeshRenderer>().sharedMaterial = section.material;
                if (colliders)
                    shape.AddComponent<MeshCollider>().convex = true;
                shape.AddComponent<FrustumShape>().Configure(section.bottomRadius, section.topRadius, section.height);
                height += section.height;
            }
        }
    }
}
