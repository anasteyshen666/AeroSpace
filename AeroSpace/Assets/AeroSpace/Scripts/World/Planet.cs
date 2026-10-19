using System;
using AeroSpace.Core;
using UnityEngine;

namespace AeroSpace.World
{
    [DefaultExecutionOrder(-2000)]
    public class Planet : MonoBehaviour
    {
        public static Planet Instance { get; private set; }

        [SerializeField] double radius = 637_100;
        [SerializeField] double surfaceGravity = 9.81;
        [SerializeField] Atmosphere atmosphere = new();

        [Header("Launch site")]
        [SerializeField] double launchLatitude = 5.0;
        [SerializeField] double launchLongitude = -53.5;

        float windHeading;

        public double Radius => radius;
        public double GravParameter => surfaceGravity * radius * radius;
        public Atmosphere Atmosphere => atmosphere;
        public Quaternion Orientation { get; private set; }
        public Vector3d LaunchSitePosition => SurfacePoint(launchLatitude, launchLongitude, 0);

        void Awake()
        {
            Instance = this;
            var up = LocalDirection(launchLatitude, launchLongitude);
            var north = LocalNorth(launchLatitude, launchLongitude);
            Orientation = Quaternion.Inverse(Quaternion.LookRotation((Vector3)north, (Vector3)up));
            windHeading = UnityEngine.Random.Range(0f, 360f);
        }

        public static Vector3d LocalDirection(double latitude, double longitude)
        {
            double lat = latitude * Math.PI / 180, lon = longitude * Math.PI / 180;
            return new Vector3d(-Math.Cos(lat) * Math.Sin(lon), Math.Sin(lat), Math.Cos(lat) * Math.Cos(lon));
        }

        static Vector3d LocalNorth(double latitude, double longitude)
        {
            double lat = latitude * Math.PI / 180, lon = longitude * Math.PI / 180;
            return new Vector3d(Math.Sin(lat) * Math.Sin(lon), Math.Cos(lat), -Math.Sin(lat) * Math.Cos(lon));
        }

        public Vector3d SurfacePoint(double latitude, double longitude, double altitude) =>
            Orientation * LocalDirection(latitude, longitude) * (radius + altitude);

        public void LatLon(Vector3d world, out double latitude, out double longitude)
        {
            var d = (Quaternion.Inverse(Orientation) * world).normalized;
            latitude = Math.Asin(Math.Clamp(d.y, -1, 1)) * 180 / Math.PI;
            longitude = Math.Atan2(-d.x, d.z) * 180 / Math.PI;
        }

        public double Altitude(Vector3d world) => world.magnitude - radius;

        public Vector3d Gravity(Vector3d world)
        {
            double r2 = world.sqrMagnitude;
            return world * (-GravParameter / (r2 * Math.Sqrt(r2)));
        }

        public void Basis(Vector3d world, out Vector3d up, out Vector3d north, out Vector3d east)
        {
            up = world.normalized;
            var axis = Orientation * new Vector3d(0, 1, 0);
            north = (axis - up * Vector3d.Dot(axis, up)).normalized;
            east = Vector3d.Cross(up, north);
        }

        public Vector3 WindAt(Vector3d world)
        {
            float speed = atmosphere.WindSpeed(Altitude(world));
            if (speed <= 0f)
                return Vector3.zero;
            speed *= 0.7f + 0.6f * Mathf.PerlinNoise(Time.time * 0.15f, 0.37f);
            Basis(world, out _, out var north, out var east);
            double h = windHeading * Math.PI / 180;
            return (Vector3)(north * Math.Cos(h) + east * Math.Sin(h)) * speed;
        }
    }
}
