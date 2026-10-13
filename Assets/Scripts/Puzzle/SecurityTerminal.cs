using UnityEngine;

/// <summary>Terminal de sécurité qui autorise l'ouverture de la porte lorsque les plaques sont actives.</summary>
public class SecurityTerminal : Interactable
{
    public DoorController door;

    Material bodyMaterial;
    Material screenMaterial;
    Light indicator;
    int appearanceState = -1;

    void Awake()
    {
        CreateVisuals();
        UpdateAppearance();
    }

    public override string Prompt
    {
        get
        {
            if (door == null || !door.AreRequiredPlatesActive())
                return "Terminal verrouillé : maintenez les deux plaques";
            return "[E] Autoriser l'ouverture de la porte";
        }
    }

    public override bool CanInteract(GameObject user)
    {
        return door != null && door.requireTerminalActivation &&
               !door.TerminalActivated && user.GetComponent<PlayerController>() != null;
    }

    public override void Interact(GameObject user)
    {
        if (door == null || !door.AreRequiredPlatesActive())
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ShowMessage("Accès refusé : EchoBot et vous devez maintenir les deux plaques.", 5f);
            return;
        }

        if (!door.AuthorizeTerminal()) return;
        UpdateAppearance();
        if (GameManager.Instance != null)
            GameManager.Instance.ShowMessage("Autorisation acceptée ! La porte est ouverte : foncez vers la sortie !", 5f);
    }

    void CreateVisuals()
    {
        bodyMaterial = CreateMaterial(new Color(0.09f, 0.14f, 0.17f), Color.black);
        screenMaterial = CreateMaterial(new Color(1f, 0.45f, 0.08f), new Color(1f, 0.35f, 0.04f) * 2f);

        CreatePart("Terminal Housing", new Vector3(0f, 0.65f, 0f), new Vector3(0.55f, 1.3f, 0.42f), bodyMaterial);
        CreatePart("Terminal Screen", new Vector3(0f, 1.02f, -0.22f), new Vector3(0.4f, 0.36f, 0.025f), screenMaterial);
        CreatePart("Terminal Reader", new Vector3(0f, 0.56f, -0.23f), new Vector3(0.26f, 0.08f, 0.025f), screenMaterial);

        GameObject lightObject = new GameObject("Terminal Status Light");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(0f, 1.45f, 0f);
        indicator = lightObject.AddComponent<Light>();
        indicator.type = LightType.Point;
        indicator.range = 4f;
        indicator.intensity = 1.5f;
    }

    GameObject CreatePart(string partName, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(transform, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(part.GetComponent<Collider>());
        return part;
    }

    static Material CreateMaterial(Color color, Color emission)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
        return material;
    }

    void UpdateAppearance()
    {
        if (screenMaterial == null) return;
        int state = door != null && door.TerminalActivated ? 2 : door != null && door.AreRequiredPlatesActive() ? 1 : 0;
        if (state == appearanceState) return;
        appearanceState = state;
        Color color = state == 2
            ? new Color(0.15f, 1f, 0.4f)
            : state == 1
                ? new Color(0.2f, 0.85f, 1f)
                : new Color(1f, 0.45f, 0.08f);
        screenMaterial.SetColor("_EmissionColor", color * 2f);
        if (indicator != null) indicator.color = color;
    }

    void Update()
    {
        UpdateAppearance();
    }

    void OnDestroy()
    {
        if (bodyMaterial != null) Destroy(bodyMaterial);
        if (screenMaterial != null) Destroy(screenMaterial);
    }
}
