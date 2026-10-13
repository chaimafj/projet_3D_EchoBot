using UnityEngine;

/// <summary>Rotation continue (ventilateurs, caméra du menu).</summary>
public class Rotator : MonoBehaviour
{
    public Vector3 degreesPerSecond = new Vector3(0f, 90f, 0f);
    public Space space = Space.Self;

    void Update()
    {
        transform.Rotate(degreesPerSecond * Time.deltaTime, space);
    }
}
