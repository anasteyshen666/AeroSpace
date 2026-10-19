using System;
using UnityEngine;

namespace AeroSpace.World
{
    [Serializable]
    public class Atmosphere
    {
        public double height = 70_000;
        public double seaLevelPressure = 101_325;
        public double seaLevelDensity = 1.225;
        public double scaleHeight = 5_600;

        [Header("Wind")]
        public float surfaceWindSpeed = 5f;
        public float peakWindSpeed = 15f;
        public float peakWindAltitude = 8_000f;
        public float windTopAltitude = 20_000f;

        public double Pressure(double altitude) => Falloff(altitude) * seaLevelPressure;
        public double Density(double altitude) => Falloff(altitude) * seaLevelDensity;
        public double PressureRatio(double altitude) => Falloff(altitude);

        public float WindSpeed(double altitude)
        {
            float h = (float)altitude;
            if (h <= peakWindAltitude)
                return Mathf.Lerp(surfaceWindSpeed, peakWindSpeed, h / peakWindAltitude);
            return Mathf.Lerp(peakWindSpeed, 0f, (h - peakWindAltitude) / (windTopAltitude - peakWindAltitude));
        }

        double Falloff(double altitude) =>
            altitude >= height ? 0 : Math.Exp(-Math.Max(altitude, 0) / scaleHeight);
    }
}
