using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EchoBot rejoue la séquence enregistrée : il est replacé au début de l'enregistrement,
/// suit les positions image par image et répète les actions (E) au bon moment.
/// À la fin, il reste immobile à sa dernière position (par ex. sur une plaque de pression).
/// </summary>
public class EchoBot : MonoBehaviour
{
    [Header("Visuel")]
    public GameObject hologramDome;
    public Renderer[] glowRenderers;
    public Light glowLight;
    public Color idleGlow = new Color(0.2f, 0.75f, 1f);
    public Color replayGlow = new Color(0.3f, 1f, 0.9f);

    public bool IsReplaying { get; private set; }
    public float Progress => frames.Count > 1 ? Mathf.Clamp01(time / frames[frames.Count - 1].time) : 0f;

    readonly List<RecordFrame> frames = new List<RecordFrame>();
    readonly List<Material> glowMaterials = new List<Material>();
    float time;
    int index;
    float domePulse;

    void Awake()
    {
        foreach (Renderer r in glowRenderers)
            if (r != null) glowMaterials.Add(r.material);
        SetHologram(false);
        SetGlow(idleGlow, 2f);
    }

    public void StartReplay(IReadOnlyList<RecordFrame> source)
    {
        frames.Clear();
        frames.AddRange(source);
        time = 0f;
        index = 0;
        IsReplaying = true;
        transform.SetPositionAndRotation(frames[0].position, frames[0].rotation);
        if (frames[0].interact) DoInteract();
        SetHologram(true);
        SetGlow(replayGlow, 4f);
        if (GameManager.Instance != null) GameManager.Instance.ShowMessage("► EchoBot rejoue votre séquence…", 2f);
    }

    public void StopReplay()
    {
        IsReplaying = false;
        SetHologram(false);
        SetGlow(idleGlow, 2f);
    }

    void Update()
    {
        if (hologramDome != null && hologramDome.activeSelf)
        {
            domePulse += Time.deltaTime;
            float s = 1f + Mathf.Sin(domePulse * 3f) * 0.03f;
            hologramDome.transform.localScale = new Vector3(3.2f * s, 1.6f * s, 3.2f * s);
            hologramDome.transform.Rotate(0f, 30f * Time.deltaTime, 0f, Space.Self);
        }

        if (!IsReplaying) return;

        time += Time.deltaTime;
        // Avancer dans la liste et déclencher les actions rencontrées
        while (index < frames.Count - 1 && frames[index + 1].time <= time)
        {
            index++;
            if (frames[index].interact) DoInteract();
        }

        if (index >= frames.Count - 1)
        {
            RecordFrame last = frames[frames.Count - 1];
            transform.SetPositionAndRotation(last.position, last.rotation);
            StopReplay();
            return;
        }

        RecordFrame a = frames[index];
        RecordFrame b = frames[index + 1];
        float segment = b.time - a.time;
        float k = segment > 0.0001f ? (time - a.time) / segment : 1f;
        transform.SetPositionAndRotation(Vector3.Lerp(a.position, b.position, k), Quaternion.Slerp(a.rotation, b.rotation, k));
    }

    void DoInteract()
    {
        Interactable target = Interactable.FindNearest(transform.position, gameObject);
        if (target != null) target.Interact(gameObject);
    }

    void SetHologram(bool on)
    {
        if (hologramDome != null) hologramDome.SetActive(on);
    }

    void SetGlow(Color color, float intensity)
    {
        foreach (Material m in glowMaterials) m.SetColor("_EmissionColor", color * intensity);
        if (glowLight != null) glowLight.color = color;
    }
}
