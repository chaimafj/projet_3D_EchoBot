using UnityEngine;

/// <summary>
/// Déplacement du technicien (ZQSD ou WASD ou flèches), relatif à la caméra.
/// E = utiliser l'objet le plus proche (console, levier).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 14f;
    public float gravity = -25f;
    public Transform cameraTransform;

    public Vector3 Velocity { get; private set; }
    public Interactable CurrentInteractable { get; private set; }

    CharacterController controller;
    MovementRecorder recorder;
    float verticalVelocity;
    Vector3 externalVelocity;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        recorder = GetComponent<MovementRecorder>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    bool CanAct => GameManager.Instance == null || GameManager.Instance.CanPlay;

    void Update()
    {
        Vector3 input = CanAct ? ReadMoveInput() : Vector3.zero;

        // Direction relative à la caméra (projetée sur le sol)
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        forward.y = 0f;
        forward = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        Vector3 move = (forward * input.z + right * input.x) * moveSpeed;
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(move.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
        }

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 total = move + externalVelocity + Vector3.up * verticalVelocity;
        controller.Move(total * Time.deltaTime);
        Velocity = move + externalVelocity;
        externalVelocity = Vector3.zero;

        CurrentInteractable = CanAct ? Interactable.FindNearest(transform.position, gameObject) : null;
        if (CanAct && Input.GetKeyDown(KeyCode.E))
        {
            if (CurrentInteractable != null) CurrentInteractable.Interact(gameObject);
            else if (GameManager.Instance != null && (recorder == null || !recorder.IsRecording))
                GameManager.Instance.ShowMessage(GameManager.Instance.RecordingUnlocked
                    ? "Rien à utiliser ici. Approchez-vous d'un levier."
                    : "Rien à utiliser ici. Allez jusqu'à la console à l'écran orange (en bas à gauche).", 3f);
            if (recorder != null) recorder.MarkInteract();
        }

        float fallLimit = GameManager.Instance != null ? GameManager.Instance.fallLimitY : -10f;
        if (transform.position.y < fallLimit) Respawn(spawnPosition, spawnRotation);
    }

    /// <summary>Accepte AZERTY (ZQSD) et QWERTY (WASD) ainsi que les flèches.</summary>
    static Vector3 ReadMoveInput()
    {
        float x = 0f, z = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
        return Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
    }

    /// <summary>Ajoute une vitesse extérieure pour cette image (tapis roulant).</summary>
    public void AddExternalVelocity(Vector3 velocity)
    {
        externalVelocity += velocity;
    }

    public void Respawn(Vector3 position, Quaternion rotation)
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = true;
        verticalVelocity = 0f;
    }
}
