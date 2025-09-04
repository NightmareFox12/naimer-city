// using UnityEngine;
// using UnityEngine.InputSystem;

// public class Player : MonoBehaviour
// {
//   [Header("Movement")]
//   public float speed = 5f;

//   [Header("Gravity")]
//   public float gravity = -9.81f;
//   public float verticalVelocity;

//   //private vars
//   private InputAction moveAction;
//   private InputAction jumpAction;
//   private InputAction sprintAction;
//   private CharacterController controller;

//   void Awake()
//   {
//     moveAction?.Enable();
//     sprintAction?.Enable();
//     jumpAction?.Enable();
//   }

//   void Start()
//   {
//     controller = GetComponent<CharacterController>();

//     moveAction = InputSystem.actions.FindAction("Move");
//     jumpAction = InputSystem.actions.FindAction("Jump");
//     sprintAction = InputSystem.actions.FindAction("Sprint");
//   }

//   void Update()
//   {
//     Vector2 moveValue = moveAction.ReadValue<Vector2>();

//     // Vector en el plano XZ
//     Vector3 move = new(moveValue.x, 0, moveValue.y);

//     // Normalizar para evitar velocidad extra en diagonal
//     if (move.magnitude > 1f) move.Normalize();

//     // Convertir a dirección local del jugador
//     move = transform.TransformDirection(move);

//     // Calcular velocidad final
//     float currentSpeed = speed * (sprintAction.IsPressed() ? 2 : 1);

//     // --- JUMP ---
//     if (jumpAction.WasPressedThisFrame() && controller.isGrounded) verticalVelocity = 3f;
//     else
//     {
//       // --- GRAVITY --- 
//       if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -1f; // mantiene pegado al suelo
//       else verticalVelocity += gravity * Time.deltaTime;
//     }

//     move.y = verticalVelocity;

//     // Aplicar movimiento
//     controller.Move(currentSpeed * Time.deltaTime * move);

//   }


//   void OnDisable()
//   {
//     // Deshabilitar la acción para evitar memory leaks
//     moveAction.Disable();
//     sprintAction.Disable();
//     jumpAction.Disable();
//   }
// }

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  [Header("Objects")]
  public Transform mainCamera;

  [Header("Movement")]
  public float moveSpeed = 3f;
  public float rotationSpeed = 12f; // Suavizado de giro del personaje
  Vector3 velocity;
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

  private CharacterController chrController;
  private InputAction moveAction;


  void Awake()
  {
    chrController = GetComponent<CharacterController>();
  }

  void Start()
  {
    var input = InputSystem.actions;
    moveAction = input.FindAction("Move");
    if (moveAction != null && !moveAction.enabled) moveAction.Enable();
  }

  void Update()
  {
    if (mainCamera == null || moveAction == null) return;

    // 1) Leer input (WASD o stick): x = A/D, y = W/S
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

    // 4) Mover
    chrController.Move(moveSpeed * Time.deltaTime * moveDir);

    // 4) Gravedad
    if (chrController.isGrounded && velocity.y < 0)
      velocity.y = -2f; // lo mantiene pegado al suelo

    velocity.y += gravity * Time.deltaTime; // acumula caída

    // 5) Mover combinando horizontal y vertical
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


}
