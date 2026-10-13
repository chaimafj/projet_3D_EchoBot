using UnityEngine;

/// <summary>
/// Tapis roulant : pousse le joueur dans la direction locale +X,
/// fait défiler les lattes et déplace les caisses (obstacles mobiles).
/// Toucher une caisse renvoie le joueur au point de reprise.
/// </summary>
public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("Longueur (X local) et largeur (Z local) du tapis.")]
    public Vector2 size = new Vector2(24f, 5f);
    public float speed = 2.5f;
    public float poweredSpeed = 3.4f;
    public PowerSwitch powerSwitch;
    public Transform[] slats;
    public Transform[] crates;
    public int extraCrateCount;
    public Vector3 crateHalfSize = new Vector3(0.65f, 0.7f, 0.65f);
    public Transform respawnPoint;

    PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        SpawnExtraCrates();
    }

    void SpawnExtraCrates()
    {
        if (extraCrateCount <= 0) return;
        if (crates == null || crates.Length == 0 || crates[0] == null)
        {
            Debug.LogError("ConveyorBelt needs at least one crate to spawn extra obstacles.", this);
            return;
        }

        var allCrates = new System.Collections.Generic.List<Transform>(crates);
        for (int i = 0; i < extraCrateCount; i++)
        {
            GameObject extra = Instantiate(crates[0].gameObject, transform, false);
            extra.name = $"Moving Crate (Extra {i + 1})";
            allCrates.Add(extra.transform);
        }

        crates = allCrates.ToArray();
        float half = size.x * 0.5f;
        for (int i = 0; i < crates.Length; i++)
        {
            Vector3 position = crates[i].localPosition;
            position.x = -half + size.x * (i + 0.5f) / crates.Length;
            crates[i].localPosition = position;
        }
    }

    void Update()
    {
        float activeSpeed = powerSwitch != null && powerSwitch.IsPowered ? poweredSpeed : speed;
        float step = activeSpeed * Time.deltaTime;
        float half = size.x * 0.5f;

        foreach (Transform slat in slats) Advance(slat, step, half);
        foreach (Transform crate in crates) Advance(crate, step, half);

        if (player == null) return;
        Vector3 local = transform.InverseTransformPoint(player.transform.position);
        bool onBelt = Mathf.Abs(local.x) <= half && Mathf.Abs(local.z) <= size.y * 0.5f && local.y < 1f && local.y > -0.5f;
        if (onBelt) player.AddExternalVelocity(transform.right * activeSpeed);

        foreach (Transform crate in crates)
        {
            // Joueur ≈ cylindre de rayon 0.35 m
            Vector3 offset = player.transform.position - crate.position;
            float along = Vector3.Dot(offset, transform.right);
            float across = Vector3.Dot(offset, transform.forward);
            if (Mathf.Abs(along) < crateHalfSize.x + 0.35f && Mathf.Abs(across) < crateHalfSize.z + 0.35f && offset.y < crateHalfSize.y)
            {
                HitPlayer();
                break;
            }
        }
    }

    static void Advance(Transform item, float step, float half)
    {
        Vector3 p = item.localPosition;
        p.x += step;
        if (p.x > half) p.x -= half * 2f;
        item.localPosition = p;
    }

    void HitPlayer()
    {
        if (respawnPoint != null) player.Respawn(respawnPoint.position, respawnPoint.rotation);
        if (GameManager.Instance != null) GameManager.Instance.ShowMessage("Percuté par une caisse ! Attendez un passage libre.", 2.5f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.up * 0.5f, new Vector3(size.x, 1f, size.y));
    }
}
