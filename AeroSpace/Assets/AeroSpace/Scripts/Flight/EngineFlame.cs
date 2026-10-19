using UnityEngine;

namespace AeroSpace.Flight
{
    [RequireComponent(typeof(MeshRenderer))]
    public class EngineFlame : MonoBehaviour
    {
        Engine engine;
        float maxLength;
        MeshRenderer flameRenderer;

        public void Configure(Engine source, float length)
        {
            engine = source;
            maxLength = length;
        }

        void Awake() => flameRenderer = GetComponent<MeshRenderer>();

        void LateUpdate()
        {
            float power = engine ? engine.CurrentThrust / engine.VacuumThrust : 0f;
            flameRenderer.enabled = power > 0.01f;
            if (!flameRenderer.enabled)
                return;
            float flicker = 0.9f + 0.2f * Mathf.PerlinNoise(Time.time * 25f, 0f);
            transform.localScale = new Vector3(1f, maxLength * power * flicker, 1f);
        }
    }
}
