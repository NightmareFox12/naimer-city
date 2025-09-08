using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  [Header("Objects")]
  public Transform mainCamera;

  [Header("Movement")]
  public float moveSpeed = 4f;
  public float rotationSpeed = 12f; //Character rotation smoothing
  public float jumpForce = 4f;
  public float gravity = -9.81f;

  [Header("Detección (Raycast)")]
  [Tooltip("Distancia máxima de detección hacia delante.")]
  public float rayDistance = 1.5f;

  [Tooltip("Altura desde el suelo para lanzar el Raycast.")]
  public float rayOriginHeight = 0.8f;
  
  [Tooltip("Capas que el Raycast puede detectar.")]
  public LayerMask rayLayers = ~0; // Por defecto, todas las capas

  [Tooltip("Dibuja la línea del raycast en la escena para depurar.")]

  // Exposición mínima de resultado sin afectar la lógica existente
  public bool ObstacleAhead { get; private set; }
  public RaycastHit lastRayHit; // Info del último impacto

  // C# Events 
  public event Action<RaycastHit> OnObstacleHit;
  public event Action OnNoObstacleHit;


  // --- private vars ---
  private CharacterController chrController;

  //input actions
  private InputAction moveAction;
  private InputAction jumpAction;
  private InputAction sprintAction;

  Vector3 velocity;

  void Awake()
  {
    moveAction?.Enable();
    sprintAction?.Enable();
    jumpAction?.Enable();

    chrController = GetComponent<CharacterController>();
  }

  void Start()
  {
    moveAction = InputSystem.actions.FindAction("Move");
    sprintAction = InputSystem.actions.FindAction("Sprint");
    jumpAction = InputSystem.actions.FindAction("Jump");

    // if (moveAction != null && !moveAction.enabled) moveAction.Enable();
  }

  void Update()
  {
    Vector2 moveInput = moveAction.ReadValue<Vector2>();

    // 2) Ejes relativos a la cámara, proyectados al plano del suelo
    Vector3 camForward = mainCamera.forward;
    camForward.y = 0f;
    camForward.Normalize();

    Vector3 camRight = mainCamera.right;
    camRight.y = 0f;
    camRight.Normalize();

    // 3) Dirección de movimiento en mundo
    Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;
    if (moveDir.sqrMagnitude > 1f) moveDir.Normalize(); // diagonales sin “boost”


    // --- JUMP ---
    if (jumpAction.WasPressedThisFrame() && chrController.isGrounded) velocity.y = jumpForce;
    else
    {
      // --- GRAVITY --- 
      if (chrController.isGrounded && velocity.y < 0) velocity.y = -1f; // mantiene pegado al suelo
      else velocity.y += gravity * Time.deltaTime;
    }

    // 4) Gravedad
    // if (chrController.isGrounded && velocity.y < 0)
    // velocity.y = -2f; // lo mantiene pegado al suelo

    // velocity.y += gravity * Time.deltaTime; // acumula caída

    // --- MOVE --- 
    if (sprintAction.IsPressed()) moveSpeed = 7f;
    else moveSpeed = 4f;

    chrController.Move((moveDir * moveSpeed + velocity) * Time.deltaTime);

    // 5) Girar hacia la dirección de movimiento (estilo ZZZ)
    if (moveDir.sqrMagnitude > 0.0001f)
    {
      Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    // Agregar un Ray Cast al player (NUEVO)
    Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
    Vector3 direction = transform.forward;

    ObstacleAhead = Physics.Raycast(
        origin,
        direction,
        out lastRayHit,
        rayDistance,
        rayLayers,
        QueryTriggerInteraction.Ignore
    );

    if (ObstacleAhead && lastRayHit.collider != null)
    {
      // Debug.Log($"Raycast tocó: {.collider.gameObject.name}", lastRayHit.collider.gameObject);
      OnObstacleHit?.Invoke(lastRayHit);
    }
    else OnNoObstacleHit?.Invoke();

    Debug.DrawRay(origin, direction * rayDistance, Color.red);
  }

  // Disable the action to prevent memory leaks
  void OnDisable()
  {
    moveAction.Disable();
    sprintAction.Disable();
    jumpAction.Disable();
  }
}
