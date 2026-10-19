using System;
using AeroSpace.Core;
using UnityEngine;

namespace AeroSpace.World
{
    public static class PlanetMesh
    {
        public static Mesh Sphere(float radius, int lonSegments, int latSegments)
        {
            int columns = lonSegments + 1;
            var vertices = new Vector3[columns * (latSegments + 1)];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];

            for (int i = 0; i <= latSegments; i++)
            {
                double lat = -90 + 180.0 * i / latSegments;
                for (int j = 0; j <= lonSegments; j++)
                {
                    double lon = -180 + 360.0 * j / lonSegments;
                    var dir = (Vector3)Planet.LocalDirection(lat, lon);
                    int k = i * columns + j;
                    vertices[k] = dir * radius;
                    normals[k] = dir;
                    uv[k] = new Vector2((float)j / lonSegments, (float)i / latSegments);
                }
            }

            var triangles = new int[lonSegments * latSegments * 6];
            int t = 0;
            for (int i = 0; i < latSegments; i++)
            {
                for (int j = 0; j < lonSegments; j++)
                {
                    int a = i * columns + j, b = a + 1, c = a + columns, d = c + 1;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = d;
                    triangles[t++] = a; triangles[t++] = d; triangles[t++] = b;
                }
            }

            var mesh = new Mesh { name = "PlanetSphere" };
            if (vertices.Length > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        public static void Patch(Mesh mesh, Planet planet, Vector3d center, float patchRadius, int rings, int segments)
        {
            planet.Basis(center, out var up, out var north, out var east);
            var vertices = new Vector3[1 + rings * segments];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            double r = planet.Radius;
            double centerU = 0;

            for (int k = 0; k <= rings; k++)
            {
                double theta = patchRadius * Math.Pow((double)k / rings, 2) / r;
                int count = k == 0 ? 1 : segments;
                for (int s = 0; s < count; s++)
                {
                    double phi = 2 * Math.PI * s / segments;
                    var dir = up * Math.Cos(theta) + (north * Math.Cos(phi) + east * Math.Sin(phi)) * Math.Sin(theta);
                    int index = k == 0 ? 0 : 1 + (k - 1) * segments + s;
                    vertices[index] = (Vector3)(dir * r - center);
                    normals[index] = (Vector3)dir;

                    planet.LatLon(dir, out double lat, out double lon);
                    double u = (lon + 180) / 360;
                    if (k == 0)
                        centerU = u;
                    else if (u - centerU > 0.5)
                        u -= 1;
                    else if (u - centerU < -0.5)
                        u += 1;
                    uv[index] = new Vector2((float)u, (float)((lat + 90) / 180));
                }
            }

            var triangles = new int[segments * 3 + (rings - 1) * segments * 6];
            int t = 0;
            for (int s = 0; s < segments; s++)
            {
                int next = (s + 1) % segments;
                triangles[t++] = 0; triangles[t++] = 1 + s; triangles[t++] = 1 + next;
            }
            for (int k = 1; k < rings; k++)
            {
                int inner = 1 + (k - 1) * segments, outer = inner + segments;
                for (int s = 0; s < segments; s++)
                {
                    int next = (s + 1) % segments;
                    triangles[t++] = inner + s; triangles[t++] = outer + s; triangles[t++] = outer + next;
                    triangles[t++] = inner + s; triangles[t++] = outer + next; triangles[t++] = inner + next;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }
    }
}
