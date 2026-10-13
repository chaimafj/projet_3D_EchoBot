using UnityEngine;

/// <summary>
/// Console d'enregistrement : le joueur l'active avec E pour débloquer le système Record / Stop / Replay.
/// </summary>
public class RecordingConsole : Interactable
{
    public Renderer screenRenderer;
    public Light screenLight;
    public Color lockedColor = new Color(1f, 0.45f, 0.1f);
    public Color activeColor = new Color(0.2f, 0.9f, 1f);

    bool used;
    Material screenMaterial;
    GameObject beacon;

    void Awake()
    {
        if (screenRenderer != null) screenMaterial = screenRenderer.material;
        SetColor(lockedColor);
        CreateBeacon();
    }

    /// <summary>Colonne de lumière + flèche au-dessus de la console tant qu'elle n'est pas activée.</summary>
    void CreateBeacon()
    {
        if (screenMaterial == null) return;
        beacon = new GameObject("Console Beacon");
        beacon.transform.SetParent(transform, false);

        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(beam.GetComponent<Collider>());
        beam.transform.SetParent(beacon.transform, false);
        beam.transform.localPosition = new Vector3(0f, 7f, 0f);
        beam.transform.localScale = new Vector3(0.25f, 4.5f, 0.25f);
        beam.GetComponent<Renderer>().sharedMaterial = screenMaterial;

        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(arrow.GetComponent<Collider>());
        arrow.transform.SetParent(beacon.transform, false);
        arrow.transform.localPosition = new Vector3(0f, 2.6f, 0f);
        arrow.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
        arrow.transform.localScale = Vector3.one * 0.5f;
        arrow.GetComponent<Renderer>().sharedMaterial = screenMaterial;
        arrow.name = "Arrow";
    }

    public override string Prompt => "[E] Activer la console d'enregistrement";

    public override bool CanInteract(GameObject user) => !used && user.GetComponent<PlayerController>() != null;

    public override void Interact(GameObject user)
    {
        used = true;
        SetColor(activeColor);
        if (beacon != null) beacon.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.UnlockRecording();
    }

    void Update()
    {
        if (screenLight != null) screenLight.intensity = 1.5f + Mathf.Sin(Time.time * 4f) * 0.3f;
        if (beacon != null && beacon.activeSelf)
        {
            Transform arrow = beacon.transform.Find("Arrow");
            if (arrow != null) arrow.localPosition = new Vector3(0f, 2.6f + Mathf.Sin(Time.time * 3f) * 0.25f, 0f);
        }
    }

    void SetColor(Color color)
    {
        if (screenMaterial != null) screenMaterial.SetColor("_EmissionColor", color * 2.5f);
        if (screenLight != null) screenLight.color = color;
    }
}
