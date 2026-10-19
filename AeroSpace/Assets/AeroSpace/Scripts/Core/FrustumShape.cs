using UnityEngine;

namespace AeroSpace.Core
{
    [ExecuteAlways, RequireComponent(typeof(MeshFilter))]
    public class FrustumShape : MonoBehaviour
    {
        [SerializeField, Min(0f)] float bottomRadius = 1f;
        [SerializeField, Min(0f)] float topRadius = 1f;
        [SerializeField, Min(0.01f)] float height = 2f;
        [SerializeField, Range(6, 48)] int segments = 24;

        Mesh mesh;
        bool dirty;

        public void Configure(float bottom, float top, float shapeHeight, int segmentCount = 24)
        {
            bottomRadius = bottom;
            topRadius = top;
            height = shapeHeight;
            segments = segmentCount;
            Rebuild();
        }

        void OnEnable() => Rebuild();
        void OnValidate() => dirty = true;

        void Update()
        {
            if (dirty)
                Rebuild();
        }

        void OnDestroy()
        {
            if (!mesh)
                return;
            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);
        }

        void Rebuild()
        {
            dirty = false;
            if (!mesh)
                mesh = new Mesh { name = "Frustum", hideFlags = HideFlags.HideAndDontSave };
            Build(mesh, bottomRadius, topRadius, height, segments);
            GetComponent<MeshFilter>().sharedMesh = mesh;
            if (TryGetComponent<MeshCollider>(out var meshCollider))
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
            }
        }

        static void Build(Mesh mesh, float r0, float r1, float h, int n)
        {
            int ring = n + 1;
            var vertices = new Vector3[ring * 4 + 2];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            var triangles = new System.Collections.Generic.List<int>(n * 12);
            float slope = (r0 - r1) / h;

            for (int s = 0; s <= n; s++)
            {
                float a = s * Mathf.PI * 2f / n;
                float c = Mathf.Cos(a), sn = Mathf.Sin(a);
                var side = new Vector3(c, slope, sn).normalized;
                vertices[s] = new Vector3(c * r0, 0f, sn * r0);
                vertices[ring + s] = new Vector3(c * r1, h, sn * r1);
                normals[s] = normals[ring + s] = side;
                uv[s] = new Vector2((float)s / n, 0f);
                uv[ring + s] = new Vector2((float)s / n, 1f);

                vertices[ring * 2 + s] = vertices[s];
                vertices[ring * 3 + s] = vertices[ring + s];
                normals[ring * 2 + s] = Vector3.down;
                normals[ring * 3 + s] = Vector3.up;
            }

            int bottomCenter = ring * 4, topCenter = ring * 4 + 1;
            vertices[bottomCenter] = Vector3.zero;
            vertices[topCenter] = new Vector3(0f, h, 0f);
            normals[bottomCenter] = Vector3.down;
            normals[topCenter] = Vector3.up;

            for (int s = 0; s < n; s++)
            {
                int b0 = s, b1 = s + 1, t0 = ring + s, t1 = ring + s + 1;
                triangles.AddRange(new[] { b0, t0, t1, b0, t1, b1 });
                if (r0 > 1e-4f)
                    triangles.AddRange(new[] { bottomCenter, ring * 2 + s, ring * 2 + s + 1 });
                if (r1 > 1e-4f)
                    triangles.AddRange(new[] { topCenter, ring * 3 + s + 1, ring * 3 + s });
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }
    }
}
