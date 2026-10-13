using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe de base des objets utilisables avec E (console, levier).
/// Le joueur ET EchoBot peuvent les utiliser.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    static readonly List<Interactable> all = new List<Interactable>();

    public float range = 2f;

    public abstract string Prompt { get; }
    public abstract bool CanInteract(GameObject user);
    public abstract void Interact(GameObject user);

    protected virtual void OnEnable() => all.Add(this);
    protected virtual void OnDisable() => all.Remove(this);

    /// <summary>Renvoie l'objet utilisable le plus proche de la position, ou null.</summary>
    public static Interactable FindNearest(Vector3 position, GameObject user)
    {
        Interactable best = null;
        float bestDistance = float.MaxValue;
        foreach (Interactable item in all)
        {
            Vector3 offset = item.transform.position - position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance <= item.range && distance < bestDistance && item.CanInteract(user))
            {
                best = item;
                bestDistance = distance;
            }
        }
        return best;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
