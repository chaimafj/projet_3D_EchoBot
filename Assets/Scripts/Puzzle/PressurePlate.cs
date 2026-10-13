using UnityEngine;

/// <summary>
/// Plaque de pression : active tant que le joueur ou EchoBot se tient dessus.
/// </summary>
public class PressurePlate : MonoBehaviour
{
    public Vector2 size = new Vector2(1.8f, 1.8f);
    public Transform pad;
    public Renderer glowRenderer;
    public Light plateLight;
    public Color idleColor = new Color(0.1f, 0.55f, 1f);
    public Color activeColor = new Color(0.2f, 1f, 0.55f);

    public bool IsActive { get; private set; }

    Material glowMaterial;
    Vector3 padUp;
    float blend;

    void Awake()
    {
        if (glowRenderer != null) glowMaterial = glowRenderer.material;
        if (pad != null) padUp = pad.localPosition;
    }

    void Update()
    {
        IsActive = false;
        foreach (PlateActivator activator in PlateActivator.All)
        {
            Vector3 local = transform.InverseTransformPoint(activator.transform.position);
            if (Mathf.Abs(local.x) <= size.x * 0.5f && Mathf.Abs(local.z) <= size.y * 0.5f && local.y > -0.5f && local.y < 1.5f)
            {
                IsActive = true;
                break;
            }
        }

        blend = Mathf.MoveTowards(blend, IsActive ? 1f : 0f, Time.deltaTime * 6f);
        Color color = Color.Lerp(idleColor, activeColor, blend);
        if (glowMaterial != null) glowMaterial.SetColor("_EmissionColor", color * Mathf.Lerp(2f, 4f, blend));
        if (plateLight != null)
        {
            plateLight.color = color;
            plateLight.intensity = Mathf.Lerp(1.5f, 3f, blend);
        }
        if (pad != null) pad.localPosition = padUp + Vector3.down * 0.05f * blend;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = IsActive ? Color.green : Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.up * 0.5f, new Vector3(size.x, 1f, size.y));
    }
}
