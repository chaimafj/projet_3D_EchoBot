using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marque un objet capable d'activer les plaques de pression (le joueur et EchoBot).
/// </summary>
public class PlateActivator : MonoBehaviour
{
    public static readonly List<PlateActivator> All = new List<PlateActivator>();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);
}
