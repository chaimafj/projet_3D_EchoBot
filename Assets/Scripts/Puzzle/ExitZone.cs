using UnityEngine;

/// <summary>Zone de sortie : le niveau est réussi quand le joueur y entre.</summary>
public class ExitZone : MonoBehaviour
{
    public Vector3 size = new Vector3(3f, 3f, 2f);

    PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (player == null || GameManager.Instance == null || GameManager.Instance.IsLevelComplete) return;
        Vector3 local = transform.InverseTransformPoint(player.transform.position);
        if (Mathf.Abs(local.x) <= size.x * 0.5f && Mathf.Abs(local.z) <= size.z * 0.5f && local.y >= -0.5f && local.y <= size.y)
            GameManager.Instance.CompleteLevel();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.up * size.y * 0.5f, size);
    }
}
