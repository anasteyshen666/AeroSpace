using UnityEngine;

namespace AeroSpace.Flight
{
    public class Decoupler : MonoBehaviour
    {
        Material smokeMaterial;
        float height, radius;

        public float EjectionImpulse { get; private set; }

        public void Configure(float impulse, Material smoke, float ringHeight, float ringRadius)
        {
            EjectionImpulse = impulse;
            smokeMaterial = smoke;
            height = ringHeight;
            radius = ringRadius;
        }

        public void Fire()
        {
            if (!smokeMaterial)
                return;

            var puff = new GameObject("SeparationPuff");
            puff.transform.SetParent(transform, false);
            puff.transform.localPosition = new Vector3(0f, height, 0f);
            var particles = puff.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.duration = 0.2f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(radius * 2f, radius * 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(radius * 0.8f, radius * 1.6f);
            main.startColor = new Color(0.92f, 0.92f, 0.92f, 0.85f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) });
            var color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            var size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.8f));

            puff.GetComponent<ParticleSystemRenderer>().sharedMaterial = smokeMaterial;
            particles.Play();
        }
    }
}
