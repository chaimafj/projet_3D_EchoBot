using UnityEngine;

/// <summary>
/// Animation procédurale simple : balancement des jambes/bras et léger rebond pendant la marche.
/// Fonctionne pour le joueur et pour EchoBot (via la vitesse mesurée).
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    public Transform body;
    public Transform leftLeg, rightLeg, leftArm, rightArm;
    public float swingAngle = 35f;
    public float stepFrequency = 9f;
    public float bobHeight = 0.05f;

    Vector3 lastPosition;
    Vector3 bodyBase;
    float phase;
    float speedSmoothed;

    void Start()
    {
        lastPosition = transform.position;
        if (body != null) bodyBase = body.localPosition;
    }

    void LateUpdate()
    {
        if (Time.deltaTime <= 0f) return;
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        lastPosition = transform.position;

        float speed = Mathf.Min(delta.magnitude / Time.deltaTime, 8f);
        speedSmoothed = Mathf.Lerp(speedSmoothed, speed, 1f - Mathf.Exp(-12f * Time.deltaTime));
        float amount = Mathf.Clamp01(speedSmoothed / 4f);
        phase += Time.deltaTime * stepFrequency * Mathf.Max(amount, 0.15f);

        float swing = Mathf.Sin(phase) * swingAngle * amount;
        SetSwing(leftLeg, swing);
        SetSwing(rightLeg, -swing);
        SetSwing(leftArm, -swing * 0.8f);
        SetSwing(rightArm, swing * 0.8f);

        if (body != null)
        {
            float idle = Mathf.Sin(Time.time * 2f) * 0.015f;
            body.localPosition = bodyBase + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * bobHeight * amount + idle);
        }
    }

    static void SetSwing(Transform limb, float angle)
    {
        if (limb != null) limb.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}
