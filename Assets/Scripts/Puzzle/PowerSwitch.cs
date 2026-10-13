using UnityEngine;

/// <summary>
/// Levier d'alimentation de l'usine. Une fois tiré (par le joueur ou EchoBot),
/// le courant reste allumé pendant <see cref="powerDuration"/> secondes.
/// </summary>
public class PowerSwitch : Interactable
{
    public float powerDuration = 10f;
    public Transform leverPivot;
    public Renderer[] poweredRenderers;
    public Light[] poweredLights;
    public Color offColor = new Color(0.6f, 0.1f, 0.05f);
    public Color onColor = new Color(0.3f, 1f, 0.3f);

    public bool IsPowered => Remaining > 0f;
    public float Remaining { get; private set; }

    Material[] materials;
    float[] lightIntensities;
    float leverAngle = -40f;

    void Awake()
    {
        materials = new Material[poweredRenderers.Length];
        for (int i = 0; i < poweredRenderers.Length; i++) materials[i] = poweredRenderers[i].material;
        lightIntensities = new float[poweredLights.Length];
        for (int i = 0; i < poweredLights.Length; i++) lightIntensities[i] = poweredLights[i].intensity;
    }

    public override string Prompt => "[E] Tirer le levier d'alimentation";

    public override bool CanInteract(GameObject user) => !IsPowered;

    public override void Interact(GameObject user)
    {
        Remaining = powerDuration;
        if (GameManager.Instance == null) return;
        bool byRobot = user.GetComponent<EchoBot>() != null;
        GameManager.Instance.ShowMessage(byRobot
            ? $"EchoBot a rétabli le courant ! {powerDuration:0} secondes avant la coupure !"
            : $"Courant rétabli pour {powerDuration:0} secondes !", 3f);
    }

    void Update()
    {
        if (IsPowered)
        {
            Remaining -= Time.deltaTime;
            if (Remaining <= 0f)
            {
                Remaining = 0f;
                if (GameManager.Instance != null) GameManager.Instance.ShowMessage("Le courant est coupé…", 2.5f);
            }
        }

        leverAngle = Mathf.MoveTowards(leverAngle, IsPowered ? 40f : -40f, 300f * Time.deltaTime);
        if (leverPivot != null) leverPivot.localRotation = Quaternion.Euler(leverAngle, 0f, 0f);

        // Clignote quand il reste peu de temps
        bool blinkOff = IsPowered && Remaining < 2.5f && Mathf.Repeat(Time.time * 6f, 1f) < 0.4f;
        Color color = IsPowered && !blinkOff ? onColor : offColor;
        foreach (Material m in materials) m.SetColor("_EmissionColor", color * (IsPowered ? 4f : 1.2f));
        for (int i = 0; i < poweredLights.Length; i++)
        {
            poweredLights[i].color = color;
            poweredLights[i].intensity = IsPowered && !blinkOff ? lightIntensities[i] : lightIntensities[i] * 0.25f;
        }
    }
}
