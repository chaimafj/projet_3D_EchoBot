using UnityEngine;

/// <summary>Fait vaciller une lumière (ambiance d'usine abandonnée).</summary>
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public float amount = 0.35f;
    public float speed = 3f;

    Light lamp;
    float baseIntensity;
    float seed;

    void Awake()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        seed = Random.value * 100f;
    }

    void Update()
    {
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        bool glitch = Mathf.PerlinNoise(seed + 7f, Time.time * 0.7f) > 0.82f && Random.value > 0.6f;
        lamp.intensity = baseIntensity * (1f - amount + n * amount) * (glitch ? 0.2f : 1f);
    }
}
