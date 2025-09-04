using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTransform;

    [Tooltip("Offset del pivote respecto al jugador (altura del hombro/cabeza).")]
    public Vector3 pivotOffset = new(0f, 1.6f, 0f);

    [Header("Órbita")]
    [Tooltip("Distancia base cámara-pivote.")]
    public float distance = 7f;
    public float minDistance = 2.0f;
    public float maxDistance = 4.0f;

    [Tooltip("Límites de inclinación vertical (pitch).")]
    public float minPitch = -30f;
    public float maxPitch = 75f;

    [Header("Input y suavizado")]
    public float mouseSensitivity = 0.1f;       // Ajusta a gusto (0.05–0.2)
    public float rotationSmooth = 0.12f;        // 0 = sin suavizado, 0.1–0.2 suele ir bien
    public float positionSmoothTime = 0.05f;    // SmoothDamp de posición

    [Header("Colisión (opcional)")]
    public LayerMask collisionMask = ~0;        // Qué capas bloquean la cámara
    public float collisionRadius = 0.2f;        // Radio del spherecast
    public float collisionBuffer = 0.1f;        // Margen para evitar clipping

    private Camera mainCamera;
    private InputAction lookAction;

    private float yaw = 0f;
    private float pitch = 15f;

    // Estados de suavizado
    private Vector3 currentPosVelocity;
    private Quaternion targetRotation;
    private float targetDistance;

    void Awake()
    {
        mainCamera = GetComponent<Camera>(); //TODO: usarlo para el futuro, por ejemplo filtros
        targetDistance = distance;
    }

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Look");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // 1) Leer input
        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 2) Separar rotación: pitch para órbita, yaw para giro horizontal
        Quaternion pitchRotation = Quaternion.Euler(pitch, 0f, 0f); // Controla altura
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);   // Controla orientación

        // 3) Punto alrededor del cual orbita la cámara
        Vector3 pivot = playerTransform.position + pivotOffset;

        // 4) Calcular posición deseada (solo pitch afecta la posición, yaw rota alrededor)
        Vector3 localOffset = pitchRotation * new Vector3(0f, 0f, -targetDistance);
        Vector3 desiredPosNoCollision = pivot + yawRotation * localOffset;

        // 5) Colisión (opcional)
        float safeDistance = distance;
        Vector3 toCam = desiredPosNoCollision - pivot;
        float desiredMagnitude = toCam.magnitude;
        if (Physics.SphereCast(pivot, collisionRadius, toCam.normalized, out RaycastHit hit, desiredMagnitude, collisionMask, QueryTriggerInteraction.Ignore))
        {
            safeDistance = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, distance);
        }
        else
        {
            safeDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        targetDistance = Mathf.Lerp(targetDistance, safeDistance, 1f - Mathf.Exp(-10f * Time.deltaTime));

        // 6) Posición final usando distancia segura
        Vector3 desiredPos = pivot + yawRotation * (pitchRotation * new Vector3(0f, 0f, -targetDistance));

        // 7) Suavizado
        transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, desiredPos, ref currentPosVelocity, positionSmoothTime), Quaternion.Slerp(transform.rotation, yawRotation * pitchRotation, rotationSmooth));

        // Alternativa: mirar al personaje
        transform.LookAt(pivot);
    }

}
