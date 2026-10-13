using UnityEngine;

/// <summary>Caméra du menu principal : léger balancement autour des personnages.</summary>
public class MenuCameraSway : MonoBehaviour
{
    public Vector3 focus = new Vector3(0f, 1f, 0f);
    public float yaw = 10f;
    public float pitch = 14f;
    public float distance = 7f;
    public float swayAngle = 12f;
    public float swaySpeed = 0.15f;

    void LateUpdate()
    {
        float y = yaw + Mathf.Sin(Time.time * swaySpeed * Mathf.PI * 2f) * swayAngle;
        Pose pose = CameraRig.ComputePose(focus, y, pitch, distance);
        transform.SetPositionAndRotation(pose.position, pose.rotation);
    }
}
