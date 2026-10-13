using UnityEngine;

/// <summary>
/// Caméra à la troisième personne en vue plongeante.
/// Clic droit maintenu + souris = tourner la caméra. Molette = zoom.
/// </summary>
public class CameraRig : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1f, 0f);
    public float distance = 12f;
    public float minDistance = 6f;
    public float maxDistance = 18f;
    public float yaw = 0f;
    public float pitch = 52f;
    public float minPitch = 25f;
    public float maxPitch = 80f;
    public float mouseSensitivity = 3f;
    public float followSharpness = 8f;

    Vector3 focus;

    void Start()
    {
        if (target != null) focus = target.position + targetOffset;
        Apply();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, minPitch, maxPitch);
        }
        distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y, minDistance, maxDistance);

        focus = Vector3.Lerp(focus, target.position + targetOffset, 1f - Mathf.Exp(-followSharpness * Time.unscaledDeltaTime));
        Apply();
    }

    void Apply()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.SetPositionAndRotation(focus - rotation * Vector3.forward * distance, rotation);
    }

    /// <summary>Pose initiale de la caméra (utilisée aussi par le générateur de scènes).</summary>
    public static Pose ComputePose(Vector3 focusPoint, float yaw, float pitch, float distance)
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        return new Pose(focusPoint - rotation * Vector3.forward * distance, rotation);
    }
}
