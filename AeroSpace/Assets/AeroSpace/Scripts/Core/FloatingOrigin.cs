using System;
using System.Collections.Generic;
using AeroSpace.World;
using UnityEngine;

namespace AeroSpace.Core
{
    [DefaultExecutionOrder(-1000)]
    public class FloatingOrigin : MonoBehaviour
    {
        public static Vector3d Offset { get; private set; }
        public static Transform Focus { get; set; }
        public static event Action<Vector3> Shifted;

        [SerializeField] float threshold = 1000f;
        [SerializeField] LayerMask ignoredLayers;

        readonly List<GameObject> roots = new();

        public static Vector3d ToWorld(Vector3 local) => Offset + local;
        public static Vector3 ToLocal(Vector3d world) => (Vector3)(world - Offset);

        void Awake()
        {
            Offset = Planet.Instance.LaunchSitePosition;
        }

        void FixedUpdate()
        {
            if (Focus && Focus.position.sqrMagnitude > threshold * threshold)
                Shift(Focus.position);
        }

        void Shift(Vector3 delta)
        {
            gameObject.scene.GetRootGameObjects(roots);
            foreach (var root in roots)
            {
                if ((ignoredLayers.value & (1 << root.layer)) != 0)
                    continue;
                root.transform.position -= delta;
                if (root.TryGetComponent<Rigidbody>(out var body))
                    body.position = root.transform.position;
            }
            Offset += delta;
            Physics.SyncTransforms();
            Shifted?.Invoke(delta);
        }
    }
}
