using UnityEngine;

namespace AeroSpace.Flight
{
    [RequireComponent(typeof(MeshRenderer))]
    public class EngineFlame : MonoBehaviour
    {
        [SerializeField] Engine engine;
        [SerializeField] float maxLength = 8f;

        MeshRenderer flameRenderer;

        void Awake() => flameRenderer = GetComponent<MeshRenderer>();

        void LateUpdate()
        {
            float power = engine.CurrentThrust / engine.VacuumThrust;
            flameRenderer.enabled = power > 0.01f;
            if (!flameRenderer.enabled)
                return;
            float flicker = 0.9f + 0.2f * Mathf.PerlinNoise(Time.time * 25f, 0f);
            transform.localScale = new Vector3(1f, maxLength * power * flicker, 1f);
        }
    }
}
