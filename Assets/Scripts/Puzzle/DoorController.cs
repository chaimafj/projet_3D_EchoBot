using UnityEngine;

/// <summary>
/// Porte coulissante. S'ouvre quand toutes les plaques requises sont actives
/// et (si défini) quand l'alimentation est allumée.
/// </summary>
public class DoorController : MonoBehaviour
{
    public PressurePlate[] requiredPlates;
    public PowerSwitch requiredPower;
    public bool requireTerminalActivation;
    [Tooltip("Une fois ouverte, la porte reste ouverte.")]
    public bool stayOpen = true;
    [Tooltip("Durée pendant laquelle la porte reste ouverte après la perte de ses conditions.")]
    public float openHoldDuration;

    [Header("Visuel")]
    public Transform leftPanel;
    public Transform rightPanel;
    public float openDistance = 1.6f;
    public float speed = 3f;
    public Renderer statusRenderer;
    public Light statusLight;

    public bool IsOpen { get; private set; }

    Vector3 leftClosed, rightClosed;
    Material statusMaterial;
    bool latched;
    public bool TerminalActivated { get; private set; }
    bool conditionsWereMet;
    float openTimer;

    void Awake()
    {
        if (leftPanel != null) leftClosed = leftPanel.localPosition;
        if (rightPanel != null) rightClosed = rightPanel.localPosition;
        if (statusRenderer != null) statusMaterial = statusRenderer.material;
    }

    bool ConditionsMet()
    {
        if (requiredPlates != null)
            foreach (PressurePlate plate in requiredPlates)
                if (plate != null && !plate.IsActive) return false;
        if (requiredPower != null && !requiredPower.IsPowered) return false;
        if (requireTerminalActivation && !TerminalActivated) return false;
        return true;
    }

    public bool AreRequiredPlatesActive()
    {
        if (requiredPlates == null || requiredPlates.Length == 0) return false;
        foreach (PressurePlate plate in requiredPlates)
            if (plate == null || !plate.IsActive) return false;
        return true;
    }

    public bool AuthorizeTerminal()
    {
        if (!requireTerminalActivation || TerminalActivated || !AreRequiredPlatesActive()) return false;
        TerminalActivated = true;
        return true;
    }

    void Update()
    {
        bool conditionsMet = ConditionsMet();
        if (conditionsMet)
        {
            openTimer = Mathf.Max(0f, openHoldDuration);
        }
        else
        {
            openTimer = Mathf.Max(0f, openTimer - Time.deltaTime);
            if (conditionsWereMet && !stayOpen && openHoldDuration > 0f && GameManager.Instance != null)
                GameManager.Instance.ShowMessage($"Alarme ! La porte se referme dans {openHoldDuration:0} secondes !", 2.5f);
        }
        conditionsWereMet = conditionsMet;

        bool open = latched || conditionsMet || openTimer > 0f;
        if (open && !IsOpen)
        {
            if (stayOpen) latched = true;
            if (!requireTerminalActivation && GameManager.Instance != null)
                GameManager.Instance.ShowMessage("La porte s'ouvre !", 2.5f);
        }
        IsOpen = open;

        float step = speed * Time.deltaTime;
        if (leftPanel != null)
            leftPanel.localPosition = Vector3.MoveTowards(leftPanel.localPosition, leftClosed + (IsOpen ? Vector3.left * openDistance : Vector3.zero), step);
        if (rightPanel != null)
            rightPanel.localPosition = Vector3.MoveTowards(rightPanel.localPosition, rightClosed + (IsOpen ? Vector3.right * openDistance : Vector3.zero), step);

        Color color = IsOpen ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.15f, 0.1f);
        if (statusMaterial != null) statusMaterial.SetColor("_EmissionColor", color * 3f);
        if (statusLight != null) statusLight.color = color;
    }
}
